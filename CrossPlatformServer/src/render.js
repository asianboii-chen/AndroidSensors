const d3 = require('d3'), fs = require('fs'), net = require('net'),
      os = require('os'), {shell} = require('electron');

/** Currently supports only one client socket. */
class Networking {
  /** @constant */
  static PORT = 6912;

  static getIPAddr() {
    let interfaces = os.networkInterfaces();
    for (let p in interfaces) {
      for (let q in interfaces[p]) {
        let addr = interfaces[p][q];
        if (addr.family == 'IPv4' && !addr['internal']) return addr.address;
      }
    }
  }

  /**
   * @protected
   * @type {net.Server}
   */
  static server;

  /** @type {function():void} */
  static onConnected;

  /** @type {function(object):void} */
  static onUpdated;

  /** @type {function():void} */
  static onDisconnected;

  static startServer(onConnected, onUpdated, onDisconnected) {
    Networking.onConnected = onConnected;
    Networking.onUpdated = onUpdated;
    Networking.onDisconnected = onDisconnected;
    Networking.server = net.createServer();
    Networking.server.maxConnections = 1;
    Networking.server.listen(Networking.PORT);
    Networking.server.on('connection', Networking.onServerConnection);
  }

  /**
   * @protected
   * @type {net.Socket}
   */
  static socket;

  /**
   * @protected
   * @param {net.Socket} socket
   */
  static onServerConnection(socket) {
    Networking.socket = socket;
    Networking.onConnected?.call(socket);
    socket.on('data', Networking.onSocketData);
    socket.on('close', Networking.onSocketClose);
  }

  /**
   * @protected
   * @param {Buffer} data
   */
  static onSocketData(data) {
    for (let token of data.toString().trim().split('\n'))
      Networking.onUpdated?.call(Networking.socket, token);
  }

  /** @protected */
  static onSocketClose() {
    Networking.onDisconnected?.call(Networking.socket);
    Networking.socket = null;
  }

  static send(data) {
    Networking.socket.write(data + '\n');
  }
}

class SensorData {
  SensorData() {
    this.seconds = 0;
    this.heartRate = 0;
    this.accelerationX = 0;
    this.accelerationY = 0;
    this.accelerationZ = 0;
    this.angularVelocityX = 0;
    this.angularVelocityY = 0;
    this.angularVelocityZ = 0;
  }

  toString() {
    return [
      this.seconds.toFixed(2),
      this.heartRate,
      this.accelerationX.toFixed(2),
      this.accelerationY.toFixed(2),
      this.accelerationZ.toFixed(2),
      this.angularVelocityX.toFixed(1),
      this.angularVelocityY.toFixed(1),
      this.angularVelocityZ.toFixed(1),
    ].join(',');
  }
}

/** @param {Date} date */
function formatDate(date) {
  return [
    (date.getFullYear() % 100).toString().padStart(2, '0'),
    (date.getMonth() + 1).toString().padStart(2, '0'),
    (date.getDate()).toString().padStart(2, '0'),
    (date.getHours()).toString().padStart(2, '0'),
    (date.getMinutes()).toString().padStart(2, '0'),
    (date.getSeconds()).toString().padStart(2, '0'),
  ].join('-');
}

d3.select('#addr-label').text(Networking.getIPAddr());

const dataDirName =
    __dirname.substr(0, __dirname.lastIndexOf('src')) + 'record/';
let audioRecorder = null, audioFileName = '';
navigator.mediaDevices.getUserMedia({audio: true}).then(stream => {
  audioRecorder = new MediaRecorder(stream);
  audioRecorder.addEventListener('dataavailable', e => {
    if (!fs.existsSync(dataDirName)) fs.mkdirSync(dataDirName);
    let reader = new FileReader();
    reader.onload = function() {
      fs.writeFile(
          audioFileName, Buffer.from(new Uint8Array(this.result)), ex => {
            if (ex) throw ex;
          });
    };
    reader.readAsArrayBuffer(e.data);
  });
});

function onConnected() {
  d3.select('#disconnected-label').style('display', 'none');
  d3.select('#connected-label').style('display', 'initial');
  d3.select('#start-button')
      .classed('btn-outline-secondary', false)
      .classed('btn-secondary', true)
      .attr('disabled', null);
}

const SENSOR_BUFFER_SIZE = 1728;
const SENSOR_DATA_HEADER = [
  'Seconds', 'Heart Rate', 'X Acceleration', 'Y Acceleration', 'Z Acceleration',
  'X Angular Velocity', 'Y Angular Velocity', 'Z Angular Velocity'
].join(',');
let sensorBuffer = [], sensorFileName = '';

function saveSensorBuffer() {
  const toSave = sensorBuffer.concat();
  sensorBuffer.length = 0;
  if (!fs.existsSync(dataDirName)) fs.mkdirSync(dataDirName);
  fs.appendFile(sensorFileName, toSave.join('\n'), ex => {
    if (ex) throw ex;
  });
}

function onUpdated(data) {
  if (!isRunning) return;

  const sensorData = Object.assign(new SensorData(), JSON.parse(data));
  sensorBuffer.push(sensorData.toString());
  if (sensorBuffer.length == SENSOR_BUFFER_SIZE) saveSensorBuffer();
}

function onDisconnected() {
  if (isRunning) {
    saveSensorBuffer();
    audioRecorder.stop();
    shell.showItemInFolder(sensorFileName);
    isRunning = false;
  }
  d3.select('#running-label').style('display', 'none');
  d3.select('#connected-label').style('display', 'none');
  d3.select('#disconnected-label').style('display', 'initial');
  d3.select('#start-button')
      .classed('btn-outline-secondary', true)
      .classed('btn-secondary', false)
      .attr('disabled', '')
      .text('Start');
}

Networking.startServer(onConnected, onUpdated, onDisconnected);

let isRunning = false;

function onStartButtonClick() {
  isRunning = !isRunning;
  if (isRunning) {
    d3.select('#start-button').text('Stop');
    d3.select('#connected-label').style('display', 'none');
    d3.select('#running-label').style('display', 'initial');

    const dateString = formatDate(new Date());
    sensorFileName = dataDirName + dateString + '-sensor.csv';
    sensorBuffer.push(SENSOR_DATA_HEADER);
    Networking.send('start 50');
    audioFileName = dataDirName + dateString + '-audio.wav';
    audioRecorder.start();
  } else {
    saveSensorBuffer();
    d3.select('#start-button').text('Start');
    d3.select('#connected-label').style('display', 'initial');
    d3.select('#running-label').style('display', 'none');
    Networking.send('stop');
    audioRecorder.stop();
    shell.openItem(dataDirName);
  }
}

d3.select('#start-button').on('click', onStartButtonClick);
