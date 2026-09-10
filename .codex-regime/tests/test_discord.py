import json
import unittest
from unittest.mock import patch

from rh_crypto_bot.discord import (
    DiscordWebhook, action_message, promotion_ready_message, tracked_markets_message,
)


class _Response:
    status = 204
    def __enter__(self):
        return self
    def __exit__(self, *args):
        return None


class DiscordTests(unittest.TestCase):
    def test_rejects_non_discord_destination(self) -> None:
        with self.assertRaises(ValueError):
            DiscordWebhook("https://example.com/api/webhooks/1/token")

    def test_sender_disables_mentions_and_does_not_expose_url(self) -> None:
        with patch("rh_crypto_bot.discord.urlopen", return_value=_Response()) as send:
            webhook = DiscordWebhook("https://discord.com/api/webhooks/1/secret")
            webhook.send("test @everyone")
        payload = json.loads(send.call_args.args[0].data)
        self.assertEqual(payload["allowed_mentions"], {"parse": []})
        self.assertNotIn("secret", payload["content"])

    def test_tracked_markets_separates_benchmark_and_altcoins(self) -> None:
        message = tracked_markets_message(["eth-usd", "BTC-USD", "ETH-USD", "SOL-USD"])
        self.assertIn("Benchmark coin: BTC-USD", message)
        self.assertIn("Altcoins (2): ETH-USD, SOL-USD", message)
        self.assertIn("Total tracked markets: 3", message)
        self.assertIn("no real orders", message)

    def test_action_message_is_quiet_without_changes(self) -> None:
        self.assertIsNone(action_message([], {}))

    def test_promotion_message_requires_complete_gate(self) -> None:
        self.assertIsNone(promotion_ready_message({"promotion_gate": {"passed": False}}))
        message = promotion_ready_message({
            "promotion_gate": {"passed": True},
            "independent_signal_results": {
                "signals": 25,
                "average_net_return": "0.0420",
                "profit_factor": "1.80",
                "maximum_drawdown": "0.0800",
                "confidence_interval_95": ["0.0100", "0.0740"],
                "average_btc_benchmark_return": "0.0100",
                "average_market_benchmark_return": "0.0050",
            },
        })
        self.assertIn("every research promotion prerequisite passed", message)
        self.assertIn("Independent signals=25", message)
        self.assertIn("profit factor=1.80", message)
        self.assertIn("maximum drawdown=0.0800", message)
        self.assertIn("Live trading remains disabled", message)

    def test_action_message_contains_only_tracking_summary(self) -> None:
        message = action_message(
            [{"symbol": "BTC-USD", "action": "closed", "reason": "target"}],
            {"open_positions": 0, "closed_positions": 1,
             "total_hypothetical_profit_loss": "7.00"},
        )
        self.assertIn("BTC-USD (target)", message)
        self.assertIn("no real orders", message)
