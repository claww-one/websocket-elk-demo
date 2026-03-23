# WebSocket ELK Log Demo (.NET 4.8)

這個專案示範如何在 Docker 環境中，讓 .NET 4.8 透過 WebSocket 將 Log 傳送到 ELK。

## 架構
- **.NET 4.8 App**: 透過 Mono 在 Docker 執行，作為 WebSocket Client。
- **Node.js Relay**: WebSocket Server，將接收到的資料轉發給 Logstash HTTP Input。
- **ELK Stack**: Logstash -> Elasticsearch -> Kibana。

## 使用步驟
1. 啟動服務：`docker-compose up -d`
2. 等待 ELK 啟動（約 2 分鐘）。
3. 進入 Kibana (`http://localhost:5601`) 建立 Index Pattern `websocket-logs-*`。
4. 在 Discover 頁面查看 Log。
