from decimal import Decimal
from pathlib import Path
from tempfile import TemporaryDirectory
import json
import unittest
from datetime import datetime
from rh_crypto_bot.professional import EventRiskCalendar, PortfolioRiskLimits, ProfessionalResearchStore, portfolio_risk_check, research_validation

class ProfessionalTests(unittest.TestCase):
    def test_portfolio_circuit_breakers(self):
        ok, reasons = portfolio_risk_check(open_notional=Decimal("900"), proposed_notional=Decimal("400"), daily_return=Decimal("-.03"), consecutive_losses=4, limits=PortfolioRiskLimits())
        self.assertFalse(ok); self.assertEqual(len(reasons), 3)
    def test_event_risk(self):
        with TemporaryDirectory() as folder:
            path=Path(folder)/"events.json"; path.write_text(json.dumps({"events":[{"start":10,"end":20,"symbols":["BTC-USD"],"reason":"event"}]}))
            self.assertEqual(EventRiskCalendar(path).active_reasons(15,"BTC-USD"),["event"])
    def test_event_risk_accepts_readable_iso_dates(self):
        with TemporaryDirectory() as folder:
            path=Path(folder)/"events.json"; path.write_text(json.dumps({"events":[{
                "start":"2026-09-16T17:00:00Z","end":"2026-09-16T20:30:00Z",
                "symbols":["ALL"],"reason":"FOMC"}]}))
            when=int(datetime.fromisoformat("2026-09-16T18:00:00+00:00").timestamp())
            self.assertEqual(EventRiskCalendar(path).active_reasons(when,"ETH-USD"),["FOMC"])
            self.assertEqual(EventRiskCalendar(path).events()[0]["start"],
                             EventRiskCalendar._timestamp("2026-09-16T17:00:00Z"))
    def test_event_risk_rejects_timezone_free_dates(self):
        with self.assertRaises(ValueError):
            EventRiskCalendar._timestamp("2026-09-16T18:00:00")
    def test_validation_deterministic(self):
        values=[Decimal(".02"),Decimal("-.01"),Decimal(".03")]
        self.assertEqual(research_validation(values,simulations=100),research_validation(values,simulations=100))
    def test_order_idempotency(self):
        with TemporaryDirectory() as folder:
            store=ProfessionalResearchStore(Path(folder)/"r.db"); store.initialize()
            a=store.simulate_order("BTC-USD",1,Decimal(".01"),"proposed","test")
            b=store.simulate_order("BTC-USD",1,Decimal(".01"),"filled","simulated")
            self.assertEqual(a,b)
