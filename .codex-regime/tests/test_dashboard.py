import sqlite3
import unittest
from contextlib import closing
from pathlib import Path
from tempfile import TemporaryDirectory

from rh_crypto_bot.dashboard import dashboard_data
from rh_crypto_bot.shadow import ShadowStore


class DashboardTests(unittest.TestCase):
    def test_empty_dashboard_is_safe_and_read_only(self):
        with TemporaryDirectory() as folder:
            result = dashboard_data(Path(folder), now=1000)
        self.assertFalse(result["orders_enabled"])
        self.assertEqual(result["performance"]["signals"], 0)
        self.assertEqual([track["id"] for track in result["tracks"]], ["strict", "balanced"])
        self.assertFalse(result["tracks"][1]["gate_passed"])
        self.assertEqual(len(result["market_status"]), 11)

    def test_dashboard_combines_health_flow_and_audit(self):
        with TemporaryDirectory() as folder:
            data = Path(folder)
            shadow = ShadowStore(data / "shadow.db")
            shadow.initialize()
            shadow.audit("BTC-USD", "no_trade", "waiting for trend")
            challenger = ShadowStore(data / "shadow-balanced.db")
            challenger.initialize()
            challenger.audit("ETH-USD", "opened", "balanced observation")
            with closing(sqlite3.connect(data / "scheduler-status.db")) as connection:
                connection.execute(
                    "CREATE TABLE task_runs(mode TEXT PRIMARY KEY,completed_at TEXT,returncode INTEGER)"
                )
                connection.execute(
                    "INSERT INTO task_runs VALUES('health','2026-09-09T12:00:00+00:00',0)"
                )
                connection.commit()
            with closing(sqlite3.connect(data / "professional.db")) as connection:
                connection.execute(
                    "CREATE TABLE stream_status(id INTEGER PRIMARY KEY,last_message_at INTEGER,reconnects INTEGER,last_error TEXT)"
                )
                connection.execute(
                    "CREATE TABLE order_flow_minutes(minute INTEGER,symbol TEXT,best_bid TEXT,best_ask TEXT,spread TEXT,bid_depth TEXT,ask_depth TEXT,imbalance TEXT,buy_notional TEXT,sell_notional TEXT,trade_count INTEGER)"
                )
                connection.execute("INSERT INTO stream_status VALUES(1,995,0,NULL)")
                connection.commit()
            result = dashboard_data(data, now=1000)
        self.assertTrue(result["connections"]["robinhood"]["ok"])
        self.assertTrue(result["connections"]["order_flow"]["ok"])
        self.assertEqual(result["market_status"][0]["detail"], "waiting for trend")
        self.assertEqual(result["tracks"][1]["name"], "Balanced Challenger")
        self.assertTrue(any(row["track"] == "Balanced" for row in result["recent_activity"]))
