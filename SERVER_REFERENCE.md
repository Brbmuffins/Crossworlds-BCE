# Crossworlds (BCE) — Server Reference
*Keep this private. Contains usernames and file locations.*

---

## The Big Picture — How Login Works

```
[Player opens game]
       ↓
[Types username + password into login screen]
       ↓
[Unity client sends POST to Auth Server at 15.204.243.36:3000/login]
       ↓
[Auth Server checks the MySQL database — does this user exist? is the password correct?]
       ↓
[Auth Server sends back a JWT token — a signed key proving who the player is]
       ↓
[Unity client connects to the Game Server on port 7777, includes the JWT token]
       ↓
[Game Server validates the token — if good, loads the player's character and spawns them]
       ↓
[Player is in the game]
```

The Auth Server and Game Server never trust the client directly.
The JWT token is the proof of identity passed between them.

---

## Users

### MySQL — Database Users

MySQL has two users. Think of MySQL like a filing cabinet.

| User | Password | What it does |
|------|----------|-------------|
| `root` | *(set during MySQL install)* | Full admin access. Only use this to manage the database itself. Never used by the game. |
| `crossworlds` | *(in `/opt/crossworlds-auth/.env` on the VPS — not stored here)* | The game's database account. The auth server logs in as this user to read/write player data. Has access to the `crossworlds` database only. |

**To log into MySQL as root:**
```bash
sudo mysql
```

**To log into MySQL as the game user:**
```bash
mysql -u crossworlds -p crossworlds
# enter password when prompted
```

---

### Linux — System Users

| User | What it does |
|------|-------------|
| `ubuntu` | Your admin account. This is you when you SSH in. Has sudo access. |
| `crossworlds-auth` | *(service user, no login)* The auth server process runs as this user for security. You never log in as it. |

**To SSH into the server:**
```bash
ssh ubuntu@15.204.243.36
```

---

## The Auth Server

**What it is:** A small Node.js app that handles player accounts. It's the only thing that talks to the accounts table in the database.

**Where it lives:** `/opt/crossworlds-auth/`

**Key files:**
```
/opt/crossworlds-auth/
├── server.js          ← the actual app code
├── .env               ← passwords and secrets (never share this file)
└── package.json
```

**The .env file contains:**
```
DB_HOST=localhost
DB_USER=crossworlds
DB_PASSWORD=<in /opt/crossworlds-auth/.env on the VPS — NOT stored in the repo>
DB_NAME=crossworlds
JWT_SECRET=<in VPS .env — never commit>
PORT=3000
```
> ⚠️ A real DB password was previously committed here in plaintext. It has been
> redacted; rotate it on the VPS if not already done (ROADMAP Q7).

**API endpoints:**
| Endpoint | Method | What it does |
|----------|--------|-------------|
| `/health` | GET | Returns `{"status":"ok"}` — just confirms it's running |
| `/register` | POST | Creates a new account. Body: `{ username, email, password }` |
| `/login` | POST | Logs in. Body: `{ username, password }`. Returns a JWT token. |

---

## The Game Server

**What it is:** Your Unity build running in headless mode (no graphics). Mirror Networking listens for player connections on port 7777.

**Where it lives:** `/game/<runid>/` — a numbered CI run dir that changes every deploy
(find it with `grep ExecStart /etc/systemd/system/crossworlds-server.service`).

**Key files:**
```
/game/<runid>/
├── CrossWords.x86_64     ← the server binary (run this)
├── CrossWords_Data/      ← game data (required, don't delete)
├── GameAssembly.so             ← compiled game code (required)
├── UnityPlayer.so              ← Unity runtime (required)
└── (log → /var/log/crossworlds.log)
```

---

## System Services

Both servers run as services that start automatically and restart if they crash.

| Service | What it runs | Auto-starts |
|---------|-------------|-------------|
| `crossworlds-auth` | Auth server (Node.js, port 3000) | ✅ Yes |
| `crossworlds-server` | Unity game server (port 7777) | ✅ Yes (won't start until binary exists) |
| `mysql` | Database | ✅ Yes |

**Useful commands:**

```bash
# Check if something is running
sudo systemctl status crossworlds-auth
sudo systemctl status crossworlds-server
sudo systemctl status mysql

# Start / stop / restart
sudo systemctl start crossworlds-auth
sudo systemctl stop crossworlds-auth
sudo systemctl restart crossworlds-auth

# Watch live logs
sudo journalctl -u crossworlds-auth -f
sudo journalctl -u crossworlds-server -f

# Watch game server log directly
tail -f /var/log/crossworlds.log

# Check auth server is responding
curl http://localhost:3000/health
```

---

## Firewall — Open Ports

```
Port 22   TCP — SSH (how you connect to the server)
Port 7777 UDP — Mirror Networking (how players connect to the game)
Port 3000 TCP — Auth server (how the game client logs in)
```

---

## Quick Reference — "What do I do if..."

| Problem | Command |
|---------|---------|
| Auth server is down | `sudo systemctl restart crossworlds-auth` |
| Game server crashed | `sudo systemctl restart crossworlds-server` |
| Check why something failed | `sudo journalctl -u crossworlds-server -n 50` |
| Deploy a new Unity build | CI does it on push to `main`. Manual: `scp` the tarball + `tools/deploy-server.sh` to the VPS, then `sudo bash deploy-server.sh` |
| Edit the auth server config | `sudo nano /opt/crossworlds-auth/.env` then `sudo systemctl restart crossworlds-auth` |
| Get into the database | `sudo mysql` then `USE crossworlds;` |
