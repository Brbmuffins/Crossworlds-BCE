# Professional Trading-System Review and Live-Use Readiness Assessment

**Assessment date:** 2026-09-09

**System:** Robinhood Crypto Recommendation Bot

**Decision:** **NO-GO for live orders or automated recommendations. Continue controlled shadow research.**

## Executive decision

The bot is a useful, security-conscious research platform. It has better foundations than many personal trading bots: real Robinhood fee and quote checks, completed-candle signals, forward shadow records, benchmark comparisons, a promotion gate, event-risk controls, portfolio constraints, continuous public order-flow collection, Discord health reporting, and—most importantly—no order-placement capability.

It is not presently a viable live trading system. This conclusion is based on observed evidence rather than a general warning:

| Live evidence | Current result | Minimum gate | Status |
|---|---:|---:|---|
| Independent closed signals | 5 | 25 minimum; 50 preferred | Fail |
| Win rate | 20% | Not a standalone gate | Weak evidence |
| Average net return per signal | -1.31% | +0.25% | Fail |
| Profit factor | 0.68 | 1.25 | Fail |
| Maximum drawdown | 18.96% | 15% maximum | Fail |
| 95% confidence interval | -8.78% to +6.15% | Positive lower bound | Fail |
| Bootstrap probability mean return is positive | 26.3% | No formal gate yet | Fail in substance |
| Largest-winner share | 100% | Must not depend on one winner | Fail |
| $100-size hypothetical P/L | -$6.56 | Positive after all costs | Fail |
| All four sizing experiments combined | -$69.88 | Diagnostic only | Negative |
| Real orders submitted | 0 | Must remain 0 | Pass |

The five signals are too few for reliable inference, but the preliminary direction is unfavorable. They do not justify relaxing safeguards. The correct professional response is to fix research integrity and operational controls, freeze strategy changes long enough to gather honest forward evidence, and introduce live capability only through progressively stricter gates.

No system can promise a 9% profit after fees. A 9% per-trade target is an exit hypothesis, not an expected return, and pushing for faster or more frequent trades generally raises turnover, spread, slippage, adverse-selection, and overfitting risk. The professional objective should be **positive risk-adjusted expectancy after costs**, bounded drawdown, and operational reliability—not a guaranteed target.

## What the bot does well

1. **Safe execution boundary.** The Robinhood client is read-only and there is no network method that can place, replace, or cancel an order.
2. **Cost-aware prices.** Entry and exit research uses quantity-specific Robinhood estimates rather than treating candle closes as executable prices.
3. **Independent-signal accounting.** The $100/$200/$300/$400 variants do not artificially quadruple the statistical sample.
4. **Forward audit trail.** Signals, vetoes, positions, notifications, scheduling outcomes, market snapshots, and simulated order states are retained locally.
5. **No look-ahead by design.** Signals use completed candles, next-bar historical entries, chronological walk-forward tests, and a protected holdout.
6. **Portfolio and event controls.** The system has spread, market-impact, correlation, open-position, daily-loss, consecutive-loss, capital, and reviewed-calendar checks.
7. **External monitoring.** Separate Discord channels cover trades, health, tracked markets, and event risk, with mentions disabled and webhook URLs redacted from logs.

These are good foundations. They should be preserved while the evidence and operational layers are strengthened.

## Critical findings

### P0 — Correctness and safety blockers

#### 1. Coinbase trade direction is reversed for aggressive-flow analysis

Coinbase documents the `market_trades.side` field as the **maker side**.[1] A maker sell means an aggressive buyer crossed the spread; a maker buy means an aggressive seller crossed it. The collector currently records `BUY` as buy notional and describes the result as aggressive-trade notional. That reverses the intended interpretation.

**Impact:** imbalance features can favor the wrong direction. Because flow currently vetoes entries rather than directly creating them, the damage is bounded, but any validation using these fields is contaminated.

**Required action:** invert maker side into taker/aggressor side, rename stored fields and documentation unambiguously, add fixture tests using Coinbase examples, version the feature definition, and begin a clean post-fix evaluation period. Do not reuse affected flow history to claim predictive performance.

#### 2. WebSocket sequence gaps and state recovery are not proven

Coinbase provides sequence numbers and warns consumers to handle dropped and out-of-order messages; the Level 2 channel is intended to maintain an order book.[1][2] The collector reconnects but does not demonstrate a sequence-gap policy, atomic resnapshot, or quarantine of potentially corrupt book state.

**Impact:** a connection can look fresh while its local book is wrong. Freshness alone is not data integrity.

**Required action:** track sequence continuity per channel/product, invalidate the affected book on a gap, resubscribe/resnapshot, suppress derived features until rebuilt, count recovery events, and alert when recovery exceeds a threshold. Acceptance requires fault-injection tests for missing, duplicate, late, malformed, oversized, and reconnect-boundary messages.

#### 3. Risk limits are not tied to reconciled account equity

The research controls use configured or modeled capital and return-based loss logic. A live system needs authoritative cash, holdings, reserved buying power, open orders, fills, fees, and account net liquidation value. It must fail closed when those disagree.

**Impact:** a percentage rule can appear satisfied while dollar exposure, accumulated fills, or actual account drawdown breaches the intended bound.

**Required action:** build a read-only reconciliation ledger before adding order submission. Every decision must use current reconciled equity and available funds, with explicit dollar caps. On startup and periodically, reconcile local intent against broker holdings and order/fill state. Any unexplained difference triggers a kill switch.

#### 4. There is no independent hard kill switch

Application checks are necessary but not sufficient. Professional electronic-trading controls include pre-trade credit/capital thresholds, erroneous-order prevention, restricted access, and post-trade reporting; SEC market-access controls are a useful engineering benchmark even though this personal spot-crypto bot is not itself a broker-dealer subject to that rule.[3]

**Required action:** add a separately persisted `LIVE_TRADING_ENABLED=false` default, a daily notional cap, maximum order count, maximum position count, per-symbol cap, daily realized-plus-unrealized loss cap, stale-data cutoff, broker-disconnect cutoff, manual emergency stop, and automatic cancel/no-new-order state. A restart must not clear a tripped kill switch.

#### 5. Previously exposed credentials must be rotated before any live phase

The Robinhood private key and Discord webhook URLs were placed in conversation history. Keeping them out of Git is good, but it does not reverse exposure.

**Required action:** revoke and replace the Robinhood API key pair and every Discord webhook before recommendation or live-pilot deployment. Store replacements in a secret manager or Windows Credential Manager, not a conversational transcript or committed file. Grant the minimum permissions available and keep withdrawal authority unavailable.

### P1 — Evidence and execution blockers

#### 6. Five signals cannot support the current statistical outputs

A bootstrap interval from five non-identically distributed crypto events is descriptive, not dependable inference. Signals also share market regimes, so “independent” means deduplicated trade events, not necessarily statistically independent observations. The one winner accounts for all gains.

Backtest selection itself creates false discoveries. Bailey and colleagues show why repeatedly selecting the best historical strategy can overfit, while the Deflated Sharpe Ratio adjusts for selection bias, non-normal returns, and multiple testing.[4][5]

**Required action:** freeze the strategy specification and register its thresholds, universe-selection policy, and cost model before further collection. Keep an immutable experiment registry of every tried variation. Report results by calendar period, market regime, symbol, and signal overlap. Require at least 50 closed forward signals for recommendation review and 100 for restricted automation review, with minimum elapsed-time and regime-diversity requirements so a burst of correlated signals cannot satisfy the gate.

#### 7. The simulated order lifecycle is not an execution simulator

Accepted shadow entries are effectively marked filled. That does not model acknowledgement delay, queue position, price movement during latency, partial fills, rejects, cancellations, rate limits, or an unknown order outcome after a timeout.

**Required action:** implement a deterministic execution state machine: intent → validation → submitted → acknowledged → partial → filled/cancelled/rejected/expired/unknown. Record client order IDs and make retries idempotent. Model latency from decision through quote, request, acknowledgement, and fill. An unknown outcome must trigger reconciliation, never blind retry.

#### 8. Coinbase research data and Robinhood execution can diverge

The signal venue is Coinbase while Robinhood is the execution venue. Cross-venue basis, timing, liquidity, outages, and product behavior can differ. Robinhood explains that crypto orders are routed to market makers and execution depends on order type and market conditions.[6]

**Required action:** record timestamped Coinbase signal state and Robinhood bid/ask/estimate together. Measure basis and decision-to-quote latency. Veto when basis is abnormal. Calibrate costs by symbol, size, hour, volatility, and order type. Coinbase flow may be a context feature, but Robinhood must remain authoritative immediately before any recommendation or order.

#### 9. Validation lacks a complete multiple-testing ledger

The repository contains several strategies, targets, holding periods, candidates, universe choices, and filters. A protected holdout helps, but manually examining results and changing the system spends statistical degrees of freedom.

**Required action:** record every experiment, including failed ones; calculate a Deflated Sharpe Ratio or another declared search-adjusted statistic; use purged/embargoed time-series validation where outcomes overlap; preserve a final untouched period; and prohibit tuning from forward-shadow results without starting a new named experiment.

#### 10. Regime and order-flow features have not demonstrated incremental value

Sophisticated inputs do not automatically improve decisions. Every filter can lower trade count and increase selection bias. The current system has not shown that regime labels, event windows, or order-flow vetoes improve net expectancy out of sample.

**Required action:** run ablation comparisons against the frozen base strategy. Promote a feature only if it improves a predeclared primary metric on unseen data without materially worsening drawdown, turnover, concentration, or calibration.

### P1 — Operational blockers

#### 11. Scheduled tasks depend on an interactive Windows login

The scan, monitor, and continuous feed are local scheduled tasks that operate only while the ADMIN session is available. A reboot without login can leave the system blind.

**Required action:** for research, configure tasks to run whether the user is logged on or not, use a dedicated least-privilege service account, set restart-on-failure and missed-start policies, and verify reboot recovery. For live use, move to a supervised service on a stable host with redundant network/power or explicitly accept that the bot fails closed whenever the PC is unavailable.

#### 12. SQLite concurrency and write volume need hardening

The continuous stream writes alongside scheduled readers. The database does not yet demonstrate write-ahead logging, bounded busy timeouts, batched writes, retention vacuuming, integrity checks, or backup/restore drills.

**Required action:** enable WAL and busy timeout, batch raw events, bound database growth, checkpoint intentionally, run periodic integrity checks, and test restoration. Migrate to PostgreSQL only if measured contention or operational needs justify it.

#### 13. Health monitoring does not fully cover the market-data pipeline

Daily health verifies signed Robinhood access and Discord delivery, but it does not prove per-symbol book correctness, sequence continuity, clock accuracy, stream uptime, database writability, scan timeliness, or successful backup. If Discord itself fails, the only evidence may be local.

**Required action:** publish a compact health state including last good scan/monitor/flow time, stale symbols, sequence recoveries, reconnects, last Robinhood success, clock offset, disk/database status, current kill-switch state, and last backup. Add a second failure path outside the same Discord dependency for live use.

#### 14. Calendar risk is curated but not operationally assured

The official Fed and BLS calendars are appropriate primary sources, but release schedules can change.[7][8] A static reviewed file is safer than silently trusting scraped content, yet it can become stale.

**Required action:** store source URL, retrieval date, reviewer, timezone, checksum, and next-review deadline. Fail closed for new entries if the calendar is past its review deadline. Include unexpected major-event and venue-incident manual blackout controls.

## Target professional operating model

The system should separate five responsibilities:

1. **Research:** immutable raw data, feature versions, experiment registry, backtests, and forward evaluation.
2. **Decision:** a deterministic, versioned strategy that emits `NO_TRADE` by default and a complete recommendation only when every prerequisite passes.
3. **Risk:** an independent pre-trade engine with broker-reconciled balances and hard caps that can veto any decision but cannot create one.
4. **Execution:** an idempotent state machine that handles acknowledgement, partial fills, unknown outcomes, cancellation, and reconciliation.
5. **Operations:** health, secrets, clock, deployment, backups, audit, alerts, incident response, and controlled release/rollback.

The risk and execution layers should not trust a strategy’s claimed position size. They independently recompute the allowable quantity from current equity, price, stop distance, liquidity, aggregate exposure, daily loss, and configured caps.

## Recommended decision metrics

Replace “will this make 9%?” with a scorecard:

- Net expectancy per trade and per unit of risk after fees, spread, slippage, and adverse fills.
- Profit factor and payoff ratio with confidence intervals.
- Maximum and expected shortfall drawdown.
- Turnover and cost as a share of gross edge.
- Benchmark-relative return, beta, and downside capture.
- Calibration: actual win frequency versus predicted probability bands.
- Concentration by symbol, regime, day, and largest winners.
- Time under water and recovery duration.
- Capacity: outcome degradation from $100 to $400 and beyond.
- Operational error rate, stale-data rate, reconciliation breaks, and recovery time.

The current 9% ceiling can remain one candidate exit, but it should not be a promotion criterion. The bot should choose among predeclared exits only if unseen evidence supports the choice; otherwise use a fixed specification.

## Staged path to live use

### Phase 0 — Preserve the safety boundary (now)

- Keep order placement absent and recommendation output disabled.
- Rotate all previously exposed credentials.
- Snapshot the current strategy specification and begin an experiment registry.
- Correct aggressive-flow direction and invalidate affected derived history.
- Implement sequence-gap detection, book rebuild, and flow-health alerts.
- Add WAL/batching/integrity checks and verify reboot recovery.

**Exit gate:** all P0 tests pass; seven consecutive days of complete, correct collection with no unexplained gaps; no real order method exists.

### Phase 1 — Validate research integrity

- Add purged/embargoed validation and search-adjusted performance reporting.
- Create feature ablations and cross-venue basis/cost studies.
- Version data, features, strategy, config, and experiment IDs in every record.
- Freeze one candidate before collecting new forward evidence.

**Exit gate:** positive untouched-period expectancy under conservative costs, no data-leakage finding, no unexplained dependence on one symbol/regime/winner, and documented rejection of weaker candidates.

### Phase 2 — Shadow execution and resilience

- Build realistic order lifecycle, latency, partial-fill, rejection, timeout, and reconciliation simulation.
- Exercise stale feeds, sequence gaps, broker outage, Discord outage, disk full, database lock, process crash, PC reboot, duplicate messages, and clock drift.
- Continue the frozen strategy to at least 50 closed forward signals over at least eight weeks and materially varied regimes.

**Exit gate:** current statistical gate passes, bootstrap/robustness results are credible for the sample, no critical operational incident remains unresolved, and results remain positive after stressed costs.

### Phase 3 — Recommendation-only pilot

- Publish a complete reproducible recommendation record, or `NO_TRADE`.
- Require explicit human accept/reject and record the decision and timing.
- Reprice immediately before display; expire stale recommendations.
- Compare recommended, accepted, declined, and realistically executable outcomes.

**Exit gate:** 50–100 manually reviewed recommendations; zero ambiguous decisions; stable calibration; documented operator runbook and incident drill. This stage still submits no order.

### Phase 4 — Manual live micro-pilot

- Add broker order capability behind compile-time/configuration/credential gates.
- Require human approval for every order.
- Use a newly funded, tightly capped amount separate from savings and essential funds.
- Allow one position at a time, no leverage, no withdrawals, and conservative order types.
- Start with the smallest Robinhood-supported notional, not $200–$400.

**Exit gate:** at least 30 reconciled live round trips and 30 calendar days; zero unknown orders or reconciliation breaks; realized costs within modeled bounds; hard loss/drawdown caps never bypassed; positive net expectancy is not contradicted by live evidence.

### Phase 5 — Restricted automation

- Require at least 100 closed forward signals in total and an independent review of code, controls, and statistics.
- Automate only the exact validated strategy and symbols.
- Retain per-order notional, daily notional, aggregate exposure, daily loss, and drawdown caps.
- Roll out one control at a time with automatic rollback and continuous reconciliation.

**Exit gate:** owner signs a written risk mandate; secrets are rotated; operational and disaster-recovery drills pass; live evidence remains positive after costs. Scaling requires a new approval and new capacity evidence.

## Live risk mandate to decide before Phase 4

The owner must explicitly set these values; they should never be inferred from the 9% target:

- Maximum total funds allocated to the bot.
- Maximum single-order and single-symbol notional.
- Maximum aggregate open exposure and number of positions.
- Maximum loss per trade in dollars and percent of reconciled equity.
- Maximum daily realized-plus-unrealized loss.
- Maximum rolling drawdown and cooldown duration after a breach.
- Approved symbols, order types, operating hours, and event blackout windows.
- Maximum acceptable spread, estimated impact, data age, and cross-venue basis.
- Who may enable live mode and how the emergency stop is invoked.

## Final recommendation

Do not optimize the current bot for more trades, a higher win rate, or a 9% promise. Optimize it for trustworthy evidence and bounded failure. The immediate professional priority is **data correctness → reconciliation and kill switches → honest forward sample → recommendation-only operation → tiny human-approved live pilot**.

If those gates are honored, the architecture can mature into a viable, tightly bounded personal trading tool. Today, it should remain an experimental shadow system.

## Sources

1. Coinbase Developer Documentation, [Advanced Trade WebSocket Channels](https://docs.cdp.coinbase.com/coinbase-business/advanced-trade-apis/websocket/websocket-channels).
2. Coinbase Developer Documentation, [WebSocket authentication and sequence guidance](https://docs.cdp.coinbase.com/coinbase-app/advanced-trade-apis/websocket/websocket-authentication).
3. U.S. Securities and Exchange Commission, [Responses to Frequently Asked Questions Concerning Risk Management Controls for Brokers or Dealers with Market Access](https://www.sec.gov/rules-regulations/staff-guidance/trading-markets-frequently-asked-questions/divisionsmarketregfaq-0).
4. Bailey, Borwein, López de Prado, and Zhu, [The Probability of Backtest Overfitting](https://www.davidhbailey.com/dhbpapers/backtest-prob.pdf).
5. Bailey and López de Prado, [The Deflated Sharpe Ratio: Correcting for Selection Bias, Backtest Overfitting and Non-Normality](https://www.davidhbailey.com/dhbpapers/deflated-sharpe.pdf).
6. Robinhood, [Crypto order routing and execution](https://robinhood.com/us/en/support/articles/360022216832/); [Robinhood Crypto API](https://robinhood.com/us/en/support/articles/crypto-api/).
7. Federal Reserve Board, [FOMC calendars](https://www.federalreserve.gov/monetarypolicy/fomccalendars.htm).
8. U.S. Bureau of Labor Statistics, [Release calendar](https://www.bls.gov/schedule/).
