import unittest
from pathlib import Path
from tempfile import TemporaryDirectory
from unittest.mock import patch

from rh_crypto_bot import scheduled


class ScheduledRunnerTests(unittest.TestCase):
    def test_safe_redacts_discord_webhook(self) -> None:
        text = "failure https://discord.com/api/webhooks/123/secret"
        self.assertNotIn("secret", scheduled._safe(text))

    def test_active_lock_skips_overlapping_run(self) -> None:
        with TemporaryDirectory() as folder, patch.object(scheduled, "DATA_DIR", Path(folder)):
            first = scheduled._acquire_lock("monitor")
            self.assertIsNotNone(first)
            self.assertIsNone(scheduled._acquire_lock("monitor"))
            descriptor, path = first
            import os

            os.close(descriptor)
            path.unlink()

    def test_health_message_reports_operations_and_safety(self) -> None:
        message = scheduled._health_message(
            {
                "open_positions": 2,
                "independent_signal_results": {"signals": 7},
                "promotion_gate": {"passed": False},
            },
            {"scan": ("2026-09-09T12:00:00+00:00", 0), "monitor": ("2026-09-09T12:05:00+00:00", 1)},
        )
        self.assertIn("Robinhood signed access: 🟢 Connected", message)
        self.assertIn("Market scan: 🟢 Healthy", message)
        self.assertIn("Position monitor: 🔴 Failed", message)
        self.assertIn("Independent signals: 7", message)
        self.assertIn("Real orders are disabled", message)

    def test_run_status_is_persisted_by_mode(self) -> None:
        with (
            TemporaryDirectory() as folder,
            patch.object(scheduled, "DATA_DIR", Path(folder)),
            patch.object(scheduled, "STATUS_DATABASE", Path(folder) / "status.db"),
        ):
            scheduled._record_run("scan", 0)
            scheduled._record_run("monitor", 1)
            self.assertEqual(scheduled._last_runs()["scan"][1], 0)
            self.assertEqual(scheduled._last_runs()["monitor"][1], 1)

    def test_calendar_message_explains_veto_and_safety(self) -> None:
        message = scheduled._calendar_message(
            [{"start": 1789563600, "end": 1789576200, "reason": "FOMC"}], "Calendar updated"
        )
        self.assertIn("FOMC", message)
        self.assertIn("pause during these windows", message)
        self.assertIn("real orders remain disabled", message)
