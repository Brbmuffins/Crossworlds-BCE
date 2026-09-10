# Development and Live-Use Roadmap

**Updated:** 2026-09-09

**Current status:** **Phase 0 — research integrity and safety remediation**

**Live-use decision:** **NO-GO. Recommendations and real orders remain disabled.**

See `PROFESSIONAL_REVIEW.md` for the evidence, rationale, and source references behind this roadmap.

## Current evidence checkpoint

| Measure | Observed | Required |
|---|---:|---:|
| Independent closed signals | 5 | 25 minimum; 50 recommendation review; 100 automation review |
| Average net return | -1.31% | At least +0.25% |
| Profit factor | 0.68 | At least 1.25 |
| Maximum drawdown | 18.96% | No more than 15% |
| 95% confidence lower bound | -8.78% | Above 0% |
| Real orders | 0 | Must remain 0 through Phase 3 |

The preliminary sample fails the promotion gate and is too small for reliable inference. Strategy changes must be registered as new experiments rather than blended into the existing result.

## Phase 0 — Research integrity and safety remediation

### P0: complete before collecting promotable evidence

- [x] Correct Coinbase `market_trades.side`: it is maker side; derive aggressor side by inversion.
- [x] Document and version order-flow fields so maker/taker meaning cannot be confused.
- [x] Quarantine historical flow features produced under the incorrect definition.
- [x] Track connection-wide WebSocket sequence continuity across interleaved channels and products.
- [x] Invalidate and rebuild all books from clean snapshots after gaps or reconnects.
- [x] Suppress flow-derived decisions until each rebuilt book is healthy.
- [ ] Complete fault tests for malformed, oversized, and live reconnect-boundary messages. Gap,
  duplicate, late-message, and clean-snapshot tests are implemented.
- [ ] Rotate the exposed Robinhood key pair and every Discord webhook.
- [ ] Move live-candidate secrets from `.env` to an OS/deployment secret store.
- [ ] Freeze and hash the current strategy, universe policy, thresholds, and cost assumptions.
- [ ] Start an append-only experiment registry containing every tried configuration and result.

### P1: operational hardening

- [ ] Enable SQLite WAL, bounded busy timeout, batched stream writes, and controlled checkpoints.
- [ ] Add database retention, integrity checks, backup, and restore verification.
- [ ] Extend health reporting with per-symbol stream age, sequence recoveries, reconnect count, scan/monitor lateness, clock offset, disk/database state, kill-switch state, and last backup.
- [ ] Configure tasks to run without an interactive login and verify reboot recovery.
- [ ] Add restart-on-failure, missed-start, and maximum-retry policies.
- [ ] Add an alert path independent of the primary Discord health webhook.
- [ ] Version the event calendar with source, retrieval time, reviewer, checksum, and review deadline; fail closed when overdue.

**Exit gate:** all P0 items complete; fault-injection tests pass; seven consecutive days of correct, complete collection; zero unexplained sequence or database integrity failures; real-order code remains absent.

## Phase 1 — Research validation

- [ ] Add purged/embargoed time-series validation for overlapping outcomes.
- [ ] Report all strategy trials and a search-adjusted statistic such as Deflated Sharpe Ratio.
- [ ] Preserve a final untouched test period and prohibit iterative tuning against it.
- [ ] Run ablations for regime, event-risk, breadth, relative-strength, and order-flow features.
- [ ] Demonstrate incremental unseen value before promoting any added feature.
- [ ] Measure Coinbase-to-Robinhood price basis, quote age, latency, spread, and estimated impact by symbol, time, volatility, and size.
- [ ] Stress fees, slippage, gaps, delayed decisions, and adverse price movement.
- [ ] Report expectancy, payoff ratio, expected shortfall, time under water, turnover/cost ratio, benchmark beta, and concentration by symbol/regime/winner.
- [ ] Freeze one candidate strategy before starting the next forward observation cohort.

**Exit gate:** positive untouched-period expectancy after conservative costs; no leakage; robustness across multiple symbols and regimes; no dependence on a single winner; documented experiment history.

## Phase 2 — Realistic shadow execution and resilience

- [x] Add an isolated balanced-challenger shadow cohort with unchanged safety vetoes and a
  side-by-side dashboard; keep strict-control promotion accounting separate.

- [ ] Implement intent → validation → submitted → acknowledged → partial → filled/cancelled/rejected/expired/unknown simulated states.
- [ ] Record idempotent client order IDs and enforce reconcile-before-retry for unknown outcomes.
- [ ] Model decision, quote, submission, acknowledgement, partial-fill, cancellation, and fill latency.
- [ ] Build a read-only broker reconciliation ledger for cash, holdings, buying power, fees, orders, and fills.
- [ ] Recompute risk from reconciled equity and explicit dollar limits.
- [ ] Add persistent kill-switch state, daily notional/order limits, per-symbol and aggregate exposure caps, daily loss cap, drawdown cap, stale-data cutoff, and broker-disconnect cutoff.
- [ ] Test Coinbase outage, Robinhood outage, Discord outage, clock drift, database lock, disk full, PC reboot, duplicate response, partial fill, timeout, and process crash.
- [ ] Collect at least 50 closed forward signals across at least eight weeks and materially different regimes.

**Statistical gate:** average net return ≥0.25%; profit factor ≥1.25; maximum drawdown ≤15%; positive 95% lower confidence bound; outperformance of fee-adjusted BTC and equal-weight market; no unacceptable winner/symbol/regime concentration.

**Exit gate:** statistical gate passes under stressed costs; all records reconcile; no unresolved critical incident; 50-signal recommendation-review sample reached. Real orders remain disabled.

## Phase 3 — Recommendation-only pilot

- [ ] Emit `NO_TRADE` by default.
- [ ] For a valid opportunity, publish entry range, target, stop, expiry, size, expected net return, expected loss, confidence/calibration band, rationale, feature/strategy version, and veto status.
- [ ] Reprice against Robinhood immediately before publication and expire stale recommendations.
- [ ] Require human accept/decline and record the response and delay.
- [ ] Compare recommended, accepted, declined, and realistically executable outcomes.
- [ ] Produce an operator runbook, incident procedure, release checklist, and rollback drill.
- [ ] Manually review 50–100 reproducible recommendations.

**Exit gate:** stable calibration and positive net expectancy; zero ambiguous or unaudited decisions; operator and incident drills pass; owner approves a written risk mandate. This phase submits no orders.

## Phase 4 — Manual live micro-pilot

- [ ] Rotate credentials again at the live boundary and grant minimum available permissions; withdrawals remain unavailable.
- [ ] Add order placement behind independent code, configuration, credential, and operator gates.
- [ ] Require human approval for every order.
- [ ] Start with the minimum practical notional, one position at a time, spot only, no leverage.
- [ ] Reconcile every acknowledgement, fill, fee, cancel, holding, and balance.
- [ ] Stop automatically on any stale feed, unknown order, reconciliation break, loss-cap breach, or operational-health failure.
- [ ] Complete at least 30 live round trips and 30 calendar days before review.

**Exit gate:** zero unknown orders or reconciliation breaks; realized execution costs remain within modeled bounds; hard caps cannot be bypassed; live evidence does not contradict positive expectancy. Scaling is not authorized.

## Phase 5 — Restricted automation

- [ ] Reach at least 100 closed forward signals in total across varied regimes.
- [ ] Obtain an independent code, security, controls, and statistical review.
- [ ] Automate only the exact validated strategy, symbols, order types, and schedule.
- [ ] Preserve per-order, daily, symbol, portfolio, loss, drawdown, stale-data, and event caps.
- [ ] Roll out one bounded capability at a time with automatic rollback.
- [ ] Require a new review and owner approval for every increase in capital or scope.

**Exit gate:** written owner risk mandate; disaster-recovery and emergency-stop drills pass; live net evidence remains positive; no unresolved high-severity finding.

## Owner decisions required before Phase 4

- [ ] Maximum total bot capital.
- [ ] Maximum order, symbol, and aggregate open notional.
- [ ] Maximum positions.
- [ ] Maximum per-trade dollar/percentage loss.
- [ ] Maximum daily realized-plus-unrealized loss.
- [ ] Maximum rolling drawdown and breach cooldown.
- [ ] Approved symbols, order types, hours, and blackout windows.
- [ ] Maximum spread, estimated impact, data age, and cross-venue basis.
- [ ] Named person authorized to enable live mode and invoke the emergency stop.

## Implemented foundation to preserve

- [x] Read-only signed Robinhood API access, account health, products, quotes, estimates, and fee rates.
- [x] Coinbase historical candle ingestion with continuity validation and no fabricated candles.
- [x] Trend/momentum, breakout, multi-timeframe regime, breadth, relative-strength, event-risk, liquidity, and correlation research controls.
- [x] Chronological walk-forward testing, protected holdout, conservative cost model, and benchmark comparison.
- [x] Four-hour forward shadow scan and five-minute open-position monitor.
- [x] Independent-signal accounting with $100/$200/$300/$400 capacity observations.
- [x] Discord trade, universe, health, and event-calendar notifications with URL redaction and mentions disabled.
- [x] Continuous Coinbase Level 2/trade/heartbeat collection with bounded reconnect and stale-flow veto.
- [x] Local audit databases, simulated order records, bootstrap and Monte Carlo diagnostics.
- [x] Promotion alerts that request manual review but cannot enable recommendations or orders.

## Permanent safety principles

- A 9% target is not a guarantee and is not the promotion objective.
- Positive risk-adjusted expectancy after all costs is the objective.
- `NO_TRADE` is the normal safe outcome.
- Robinhood is authoritative for executable prices and account state.
- Research feeds may inform a decision but may never override stale or adverse Robinhood data.
- The strategy proposes; the independent risk layer may only reduce or reject.
- A restart never clears a kill switch or unresolved order state.
- No credentials, account data, or webhook URLs are committed or printed.
- No live capital increase occurs automatically.
