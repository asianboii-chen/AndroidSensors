const {app, BrowserWindow} = require('electron');

function createWindow() {
  const window = new BrowserWindow({
    width: 576,
    height: 432,
    maximizable: false,
    resizable: false,
    webPreferences: {nodeIntegration: true},
  });
  window.removeMenu();
  window.loadFile('src/index.html');
  // window.webContents.openDevTools();
};

if (require('electron-squirrel-startup')) app.quit();

app.on('ready', () => createWindow());
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow();
});
