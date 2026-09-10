from decimal import Decimal
from pathlib import Path
import sqlite3
from tempfile import TemporaryDirectory
import unittest

from rh_crypto_bot.shadow import (
    ShadowStore, execution_price, performance_statistics, quantity_for_notional, shadow_levels,
)


class ShadowTests(unittest.TestCase):
    def test_extracts_spread_inclusive_prices(self) -> None:
        payload = {"results": [{
            "ask_inclusive_of_buy_spread": "102.50",
            "bid_inclusive_of_sell_spread": "101.25",
        }]}
        self.assertEqual(execution_price(payload, "ask"), Decimal("102.50"))
        self.assertEqual(execution_price(payload, "bid"), Decimal("101.25"))

    def test_quantity_is_rounded_down(self) -> None:
        self.assertEqual(
            quantity_for_notional(Decimal("100"), Decimal("30000")),
            Decimal("0.00333333"),
        )

    def test_performance_statistics_include_profit_factor_and_drawdown(self) -> None:
        profit_factor, drawdown = performance_statistics([
            Decimal("0.10"), Decimal("-0.05"), Decimal("0.02"), Decimal("-0.01"),
        ])
        self.assertEqual(profit_factor.quantize(Decimal("0.01")), Decimal("2.00"))
        self.assertEqual(drawdown.quantize(Decimal("0.0001")), Decimal("0.0500"))

    def test_performance_statistics_handle_no_losses(self) -> None:
        profit_factor, drawdown = performance_statistics([Decimal("0.01"), Decimal("0.02")])
        self.assertIsNone(profit_factor)
        self.assertEqual(drawdown, Decimal("0"))

    def test_position_is_idempotent_and_closes_after_costs(self) -> None:
        with TemporaryDirectory() as folder:
            store = ShadowStore(Path(folder) / "shadow.db")
            store.initialize()
            values = dict(
                symbol="BTC-USD", signal_timestamp=100, entry_price=Decimal("100"),
                quantity=Decimal("1"), requested_notional=Decimal("100"),
                target_price=Decimal("110"),
                stop_price=Decimal("97"), expires_at=200, fee_rate=Decimal("0.01"),
                rationale="test",
            )
            self.assertTrue(store.open_position(**values))
            self.assertFalse(store.open_position(**values))
            position = store.open_positions()[0]
            store.close_position(position, Decimal("110"), "target")
            summary = store.summary()
            self.assertEqual(summary["closed_positions"], 1)
            self.assertEqual(summary["wins"], 1)
            self.assertEqual(summary["size_comparison"][0]["requested_notional"], "100")
            self.assertFalse(summary["promotion_gate"]["passed"])
            self.assertEqual(summary["independent_signal_results"]["signals"], 1)
            self.assertIn("profit_factor", summary["independent_signal_results"])
            self.assertIn("maximum_drawdown", summary["independent_signal_results"])

    def test_same_signal_accepts_each_requested_size(self) -> None:
        with TemporaryDirectory() as folder:
            store = ShadowStore(Path(folder) / "shadow.db")
            store.initialize()
            for size in (Decimal("100"), Decimal("200"), Decimal("300"), Decimal("400")):
                self.assertTrue(store.open_position(
                    symbol="ETH-USD", signal_timestamp=500, entry_price=Decimal("10"),
                    quantity=size / Decimal("10"), requested_notional=size,
                    target_price=Decimal("11"), stop_price=Decimal("9.7"),
                    expires_at=900, fee_rate=Decimal("0.01"), rationale="same event",
                ))
            self.assertEqual(len(store.open_positions()), 4)
            self.assertFalse(store.signal_allowed("ETH-USD", 501, 100))
            open_notional, _, _ = store.portfolio_risk_state()
            self.assertEqual(open_notional, Decimal("400"))

    def test_marks_open_position_for_unrealized_reporting(self) -> None:
        with TemporaryDirectory() as folder:
            store = ShadowStore(Path(folder) / "shadow.db")
            store.initialize()
            store.open_position(
                symbol="BTC-USD", signal_timestamp=100, entry_price=Decimal("100"),
                quantity=Decimal("1"), requested_notional=Decimal("100"),
                target_price=Decimal("110"), stop_price=Decimal("97"), expires_at=200,
                fee_rate=Decimal("0.01"), rationale="test",
            )
            position = store.open_positions()[0]
            store.mark_position(position.id, Decimal("105"), 150)
            report = store.summary()
            self.assertEqual(len(report["open_unrealized"]), 1)
            self.assertEqual(report["open_unrealized"][0]["marked_at"], 150)

    def test_notification_marker_is_persistent_and_idempotent(self) -> None:
        with TemporaryDirectory() as folder:
            store = ShadowStore(Path(folder) / "shadow.db")
            store.initialize()
            self.assertFalse(store.notification_sent("promotion_gate_passed_v1"))
            store.mark_notification_sent("promotion_gate_passed_v1")
            store.mark_notification_sent("promotion_gate_passed_v1")
            self.assertTrue(store.notification_sent("promotion_gate_passed_v1"))

    def test_existing_single_size_database_is_migrated(self) -> None:
        with TemporaryDirectory() as folder:
            path = Path(folder) / "shadow.db"
            connection = sqlite3.connect(path)
            connection.execute("""
                CREATE TABLE shadow_positions (
                    id INTEGER PRIMARY KEY, symbol TEXT NOT NULL, signal_timestamp INTEGER NOT NULL,
                    opened_at INTEGER NOT NULL, entry_price TEXT NOT NULL, quantity TEXT NOT NULL,
                    notional TEXT NOT NULL, target_price TEXT NOT NULL, stop_price TEXT NOT NULL,
                    expires_at INTEGER NOT NULL, fee_rate TEXT NOT NULL, rationale TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'open', closed_at INTEGER, exit_price TEXT,
                    net_return TEXT, profit_loss TEXT, exit_reason TEXT,
                    UNIQUE(symbol, signal_timestamp))
            """)
            connection.commit()
            connection.close()
            store = ShadowStore(path)
            store.initialize()
            connection = sqlite3.connect(path)
            columns = [row[1] for row in connection.execute(
                "PRAGMA table_info(shadow_positions)"
            )]
            connection.close()
            self.assertIn("requested_notional", columns)

    def test_target_includes_fees(self) -> None:
        target, stop = shadow_levels(
            Decimal("100"), Decimal("0.07"), Decimal("0.03"), Decimal("0.01")
        )
        self.assertGreater(target, Decimal("107"))
        self.assertGreater(stop, Decimal("97"))


if __name__ == "__main__":
    unittest.main()
