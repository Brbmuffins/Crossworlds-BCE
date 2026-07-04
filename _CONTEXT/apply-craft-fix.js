#!/usr/bin/env node
// apply-craft-fix.js — surgically replace the POST /api/craft handler in server.js.
//
//   node apply-craft-fix.js <server.js> <professions-craft-fix.js>
//
// Finds every `app.post('/api/craft', …)` statement (there may be more than one —
// the original deploy inserted a second handler without removing the first, and
// Express only ever runs the first-registered match). Replaces the FIRST with the
// corrected handler and comments out any duplicates so exactly one remains.
//
// String/comment aware so braces inside SQL strings, template literals, and
// comments don't throw off statement-boundary detection.

const fs = require('fs');

const serverPath  = process.argv[2];
const handlerPath = process.argv[3];
if (!serverPath || !handlerPath) {
  console.error('usage: node apply-craft-fix.js <server.js> <craft-fix.js>');
  process.exit(1);
}

let src        = fs.readFileSync(serverPath, 'utf8');
const newBlock = fs.readFileSync(handlerPath, 'utf8').trim();

// Walk from the `app.post(` index to the end of the statement (the closing `);`).
function findStatementEnd(s, start) {
  let depth = 0, started = false;
  let inStr = null, esc = false, inLine = false, inBlock = false;

  for (let i = start; i < s.length; i++) {
    const c = s[i], n = s[i + 1];

    if (inLine)  { if (c === '\n') inLine = false; continue; }
    if (inBlock) { if (c === '*' && n === '/') { inBlock = false; i++; } continue; }
    if (inStr) {
      if (esc)          { esc = false; continue; }
      if (c === '\\')   { esc = true;  continue; }
      if (c === inStr)  { inStr = null; }
      continue;
    }

    if (c === '/' && n === '/') { inLine = true;  i++; continue; }
    if (c === '/' && n === '*') { inBlock = true; i++; continue; }
    if (c === "'" || c === '"' || c === '`') { inStr = c; continue; }

    if (c === '{') { depth++; started = true; }
    else if (c === '}') {
      depth--;
      if (started && depth === 0) {
        // Matched the handler body's closing brace — now find `)` then `;`.
        let j = i + 1;
        while (j < s.length && /\s/.test(s[j])) j++;
        if (s[j] === ')') {
          j++;
          while (j < s.length && /\s/.test(s[j])) j++;
          if (s[j] === ';') return j;
        }
        return i; // malformed tail — bail at the brace
      }
    }
  }
  return -1;
}

const re = /app\.post\(\s*['"]\/api\/craft['"]/g;
const matches = [];
let m;
while ((m = re.exec(src)) !== null) {
  const end = findStatementEnd(src, m.index);
  if (end === -1) {
    console.error(`[craft-fix] could not find end of handler at index ${m.index}`);
    process.exit(1);
  }
  matches.push([m.index, end]);
}

if (matches.length === 0) {
  console.error('[craft-fix] no POST /api/craft handler found in server.js');
  process.exit(1);
}

matches.sort((a, b) => a[0] - b[0]);

// Rewrite back-to-front so earlier indices stay valid.
for (let k = matches.length - 1; k >= 0; k--) {
  const [s0, e0] = matches[k];
  if (k === 0) {
    src = src.slice(0, s0) + newBlock + src.slice(e0 + 1);
  } else {
    const removed = src.slice(s0, e0 + 1);
    src = src.slice(0, s0)
        + '/* craft-fix: removed duplicate /api/craft handler\n'
        + removed.replace(/\*\//g, '*\\/')
        + '\n*/'
        + src.slice(e0 + 1);
  }
}

fs.writeFileSync(serverPath, src);
console.log(`[craft-fix] replaced ${matches.length} /api/craft handler(s); one corrected handler now active.`);
