const WebSocket = require('ws');
const axios = require('axios');

const wss = new WebSocket.Server({ port: 8081 });
const LOGSTASH_URL = process.env.LOGSTASH_URL;

wss.on('connection', (ws) => {
    console.log('App Connected');
    ws.on('message', async (data) => {
        try {
            const logEntry = JSON.parse(data);
            await axios.post(LOGSTASH_URL, logEntry);
        } catch (e) { console.error('Forward failed', e.message); }
    });
});
console.log('WS Relay running on port 8081');
