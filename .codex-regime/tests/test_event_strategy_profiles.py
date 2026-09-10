import unittest

from rh_crypto_bot.event_strategy import event_strategy_profile


class EventStrategyProfileTests(unittest.TestCase):
    def test_balanced_only_relaxes_alpha_filters(self) -> None:
        strict = event_strategy_profile("strict")
        balanced = event_strategy_profile("balanced")

        self.assertLess(balanced.breakout_window, strict.breakout_window)
        self.assertLess(balanced.atr_multiplier, strict.atr_multiplier)
        self.assertLess(balanced.volume_multiplier, strict.volume_multiplier)
        self.assertLess(balanced.minimum_breadth, strict.minimum_breadth)
        self.assertLess(
            balanced.minimum_relative_strength,
            strict.minimum_relative_strength,
        )
        self.assertEqual(
            balanced.expected_interval_seconds,
            strict.expected_interval_seconds,
        )

    def test_unknown_profile_is_rejected(self) -> None:
        with self.assertRaises(ValueError):
            event_strategy_profile("unregistered")
