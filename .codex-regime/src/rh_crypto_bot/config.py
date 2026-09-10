from __future__ import annotations

from dataclasses import dataclass
from decimal import Decimal
import os
from pathlib import Path


def _load_local_env(path: Path = Path(".env")) -> None:
    """Load a simple local .env without overriding deployment-provided values."""
    if not path.is_file():
        return
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        os.environ.setdefault(key.strip(), value.strip().strip('"').strip("'"))


@dataclass(frozen=True)
class Settings:
    api_key: str
    private_key_base64: str
    api_version: str = "v2"
    api_base_url: str = "https://trading.robinhood.com"
    mode: str = "read_only"
    target_net_return: Decimal = Decimal("0.09")
    max_risk_per_trade: Decimal = Decimal("0.005")
    max_portfolio_drawdown: Decimal = Decimal("0.05")
    discord_webhook_url: str = ""
    discord_health_webhook_url: str = ""
    discord_calendar_webhook_url: str = ""

    @classmethod
    def from_environment(cls, *, require_credentials: bool = True) -> "Settings":
        _load_local_env()
        api_key = os.getenv("ROBINHOOD_API_KEY", "").strip()
        private_key = os.getenv("ROBINHOOD_PRIVATE_KEY_BASE64", "").strip()
        if require_credentials and (not api_key or not private_key):
            raise ValueError(
                "Missing ROBINHOOD_API_KEY or ROBINHOOD_PRIVATE_KEY_BASE64. "
                "Configure a read-only key in .env or the deployment secret manager."
            )
        mode = os.getenv("BOT_MODE", "read_only").strip().lower()
        if mode != "read_only":
            raise ValueError("This release supports BOT_MODE=read_only only.")
        version = os.getenv("ROBINHOOD_API_VERSION", "v2").strip().lower()
        if version not in {"v1", "v2"}:
            raise ValueError("ROBINHOOD_API_VERSION must be v1 or v2.")
        return cls(
            api_key=api_key,
            private_key_base64=private_key,
            api_version=version,
            api_base_url=os.getenv(
                "ROBINHOOD_API_BASE_URL", "https://trading.robinhood.com"
            ).rstrip("/"),
            mode=mode,
            target_net_return=Decimal(os.getenv("TARGET_NET_RETURN", "0.09")),
            max_risk_per_trade=Decimal(os.getenv("MAX_RISK_PER_TRADE", "0.005")),
            max_portfolio_drawdown=Decimal(
                os.getenv("MAX_PORTFOLIO_DRAWDOWN", "0.05")
            ),
            discord_webhook_url=os.getenv("DISCORD_WEBHOOK_URL", "").strip(),
            discord_health_webhook_url=os.getenv("DISCORD_HEALTH_WEBHOOK_URL", "").strip(),
            discord_calendar_webhook_url=os.getenv("DISCORD_CALENDAR_WEBHOOK_URL", "").strip(),
        )
