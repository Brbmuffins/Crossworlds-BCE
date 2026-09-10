import sqlite3
import unittest
from contextlib import closing
from decimal import Decimal
from pathlib import Path
from tempfile import TemporaryDirectory

from rh_crypto_bot.order_flow import (
    FLOW_FEATURE_VERSION,
    MinuteFlow,
    SequenceGapError,
    SequenceGuard,
    ensure_feature_version,
    process_payload,
)
from rh_crypto_bot.professional import ProfessionalResearchStore


class OrderFlowTests(unittest.TestCase):
    def test_inverts_coinbase_maker_side_to_aggressor(self):
        flow = MinuteFlow(["BTC-USD"])
        flow.update_book(
            "BTC-USD",
            [
                {"side": "bid", "price_level": "99", "new_quantity": "2"},
                {"side": "offer", "price_level": "101", "new_quantity": "1"},
            ],
            snapshot=True,
        )
        flow.update_trades(
            "BTC-USD",
            [
                {"side": "SELL", "price": "100", "size": ".5"},
                {"side": "BUY", "price": "100", "size": ".25"},
            ],
        )
        row = flow.rows(60)[0]
        self.assertEqual(Decimal(row[7]), Decimal(97) / Decimal(299))
        self.assertEqual(row[8], "50.0")
        self.assertEqual(row[9], "25.00")
        self.assertEqual(row[10], 2)

    def test_book_requires_snapshot_and_snapshot_replaces_state(self):
        flow = MinuteFlow(["BTC-USD"])
        flow.update_book("BTC-USD", [{"side": "bid", "price_level": "90", "new_quantity": "1"}])
        self.assertEqual(flow.rows(60), [])
        flow.update_book(
            "BTC-USD",
            [
                {"side": "bid", "price_level": "99", "new_quantity": "1"},
                {"side": "offer", "price_level": "101", "new_quantity": "1"},
            ],
            snapshot=True,
        )
        self.assertNotIn(Decimal(90), flow.books["BTC-USD"]["bid"])
        self.assertEqual(len(flow.rows(60)), 1)

    def test_sequence_gap_raises_and_duplicate_or_late_message_is_ignored(self):
        guard = SequenceGuard()
        self.assertTrue(guard.accept(10))
        self.assertTrue(guard.accept(11))
        self.assertFalse(guard.accept(11))
        self.assertFalse(guard.accept(9))
        with self.assertRaises(SequenceGapError):
            guard.accept(13)

    def test_channel_sequence_accepts_interleaved_products(self):
        flow, guard = MinuteFlow(["BTC-USD", "ETH-USD"]), SequenceGuard()
        for sequence, symbol in ((0, "BTC-USD"), (1, "ETH-USD"), (2, "BTC-USD")):
            process_payload(flow, guard, {
                "channel": "l2_data",
                "sequence_num": sequence,
                "events": [{
                    "type": "snapshot" if sequence < 2 else "update",
                    "product_id": symbol,
                    "updates": [
                        {"side": "bid", "price_level": "99", "new_quantity": "1"},
                        {"side": "offer", "price_level": "101", "new_quantity": "1"},
                    ],
                }],
            })
        self.assertEqual(len(flow.rows(60)), 2)

    def test_feed_sequence_includes_other_channels(self):
        flow, guard = MinuteFlow(["BTC-USD"]), SequenceGuard()
        process_payload(flow, guard, {"channel": "subscriptions", "sequence_num": 0, "events": []})
        process_payload(flow, guard, {"channel": "heartbeats", "sequence_num": 1, "events": []})
        process_payload(flow, guard, {"channel": "market_trades", "sequence_num": 2, "events": []})
        process_payload(flow, guard, {"channel": "l2_data", "sequence_num": 3, "events": [{
            "type": "snapshot", "product_id": "BTC-USD", "updates": [
                {"side": "bid", "price_level": "99", "new_quantity": "1"},
                {"side": "offer", "price_level": "101", "new_quantity": "1"},
            ],
        }]})
        self.assertEqual(len(flow.rows(60)), 1)

    def test_out_of_order_update_is_not_applied(self):
        flow, guard = MinuteFlow(["BTC-USD"]), SequenceGuard()
        process_payload(
            flow,
            guard,
            {
                "channel": "l2_data",
                "sequence_num": 3,
                "events": [
                    {
                        "type": "snapshot",
                        "product_id": "BTC-USD",
                        "updates": [
                            {"side": "bid", "price_level": "99", "new_quantity": "1"},
                            {"side": "offer", "price_level": "101", "new_quantity": "1"},
                        ],
                    }
                ],
            },
        )
        process_payload(
            flow,
            guard,
            {
                "channel": "l2_data",
                "sequence_num": 2,
                "events": [
                    {
                        "type": "update",
                        "product_id": "BTC-USD",
                        "updates": [
                            {"side": "bid", "price_level": "99", "new_quantity": "9"},
                        ],
                    }
                ],
            },
        )
        self.assertEqual(flow.books["BTC-USD"]["bid"][Decimal(99)], Decimal(1))

    def test_feature_version_discards_incompatible_derived_minutes_once(self):
        with TemporaryDirectory() as folder:
            path = Path(folder) / "professional.db"
            ProfessionalResearchStore(path).initialize()
            with closing(sqlite3.connect(path)) as connection:
                connection.execute(
                    "INSERT INTO order_flow_minutes VALUES(?,?,?,?,?,?,?,?,?,?,?)",
                    (60, "BTC-USD", "99", "101", ".02", "99", "101", "-.01", "5", "6", 2),
                )
                connection.commit()
            self.assertTrue(ensure_feature_version(path))
            with closing(sqlite3.connect(path)) as connection:
                count = connection.execute("SELECT COUNT(*) FROM order_flow_minutes").fetchone()[0]
                version = connection.execute(
                    "SELECT value FROM order_flow_metadata WHERE key='feature_version'"
                ).fetchone()[0]
            self.assertEqual(count, 0)
            self.assertEqual(version, FLOW_FEATURE_VERSION)
            self.assertFalse(ensure_feature_version(path))
