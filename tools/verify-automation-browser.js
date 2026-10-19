'use strict';

// Dedicated local page only: no personal profile, network requests or hooks.
const path = require('path');
const fs = require('fs');
const os = require('os');

if (process.versions.electron) {
  const { app, BrowserWindow } = require('electron');
  app.setPath('userData', process.env.PET_AUTOMATION_FIXTURE_DATA);
  app.commandLine.appendSwitch('force-renderer-accessibility');
  app.whenReady().then(async () => {
    const window = new BrowserWindow({ width: 760, height: 480, autoHideMenuBar: true, webPreferences: { sandbox: true } });
    await window.loadURL('data:text/html;charset=utf-8,' + encodeURIComponent(`<!doctype html><html lang="en"><head><title>PetAI automation Chromium fixture</title></head><body>
      <h1>Local automation test</h1><p>This window contains test controls only.</p>
      <button onclick="location.reload()">Reload fixture page</button>
      <button id="delayed" disabled onclick="document.getElementById('result').textContent='Fixture action complete'">Delayed fixture action</button>
      <p id="result" role="status">Waiting for fixture action</p>
      <script>setTimeout(()=>document.getElementById('delayed').disabled=false,900)</script>
      </body></html>`));
    window.focus();
    process.stdout.write('FIXTURE_READY\n');
  });
  app.on('window-all-closed', () => app.quit());
} else {
  const assert = require('assert');
  const { spawn } = require('child_process');
  const electron = require('electron');
  async function main() {
    const temporaryRoot = await fs.promises.realpath(os.tmpdir());
    const data = await fs.promises.mkdtemp(path.join(temporaryRoot, 'pet-chromium-fixture-'));
    const env = { ...process.env, PET_AUTOMATION_FIXTURE_DATA: data, APPDATA: data, LOCALAPPDATA: data };
    delete env.ELECTRON_RUN_AS_NODE;
    const child = spawn(electron, ['--user-data-dir=' + data, '--disable-gpu', __filename], { windowsHide: true, shell: false, env, stdio: ['pipe', 'pipe', 'pipe'] });
    let helper;
    try {
      await new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error('Chromium fixture startup timeout')), 20000);
        let output = '', errors = '';
        child.once('error', error => { clearTimeout(timeout); reject(error); });
        child.once('exit', code => { clearTimeout(timeout); reject(new Error('Chromium fixture exited: ' + code + '\n' + errors)); });
        child.stdout.on('data', chunk => { output += chunk; if (output.includes('FIXTURE_READY')) { clearTimeout(timeout); resolve(); } });
        child.stderr.on('data', chunk => { errors += chunk; });
      });
      const helperScript = path.join(__dirname, 'automation-helper.ps1');
      const output = await new Promise((resolve, reject) => {
        helper = spawn('powershell.exe', ['-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-File', helperScript, '-BrowserTestProcessId', String(child.pid)], { windowsHide: true, shell: false });
        let stdout = '', stderr = '';
        const timeout = setTimeout(() => { helper.kill(); reject(new Error('Chromium UIA test timeout')); }, 25000);
        helper.stdout.on('data', chunk => { stdout += chunk; });
        helper.stderr.on('data', chunk => { stderr += chunk; });
        helper.once('error', error => { clearTimeout(timeout); reject(error); });
        helper.once('exit', code => { clearTimeout(timeout); code === 0 ? resolve(stdout) : reject(new Error(stderr || stdout || 'UIA test failed')); });
      });
      const events = output.trim().split(/\r?\n/).map(line => JSON.parse(line));
      assert(events.some(event => event.id === 'chromium-fixture' && event.ok && event.completedSteps === 2), output);
      assert(events.some(event => event.test === 'chromium_reload_legacy_task_and_delayed_target' && event.ok), output);
      assert(events.some(event => event.id === 'chromium-failure-fixture' && !event.ok && event.reasonCode === 'target_not_found' && event.failedStep === 1), output);
      console.log(output.trim());
      console.log('Chromium automation verification passed');
    } finally {
      if (helper && helper.exitCode === null) helper.kill();
      if (child.exitCode === null) child.kill();
      await new Promise(resolve => setTimeout(resolve, 500));
      const resolved = await fs.promises.realpath(data);
      if (path.dirname(resolved).toLowerCase() !== temporaryRoot.toLowerCase() || !path.basename(resolved).startsWith('pet-chromium-fixture-')) throw new Error('Refusing cleanup outside the dedicated fixture directory');
      await fs.promises.rm(resolved, { recursive: true, force: true, maxRetries: 5, retryDelay: 200 });
    }
  }
  main().catch(error => { console.error(error); process.exitCode = 1; });
}
