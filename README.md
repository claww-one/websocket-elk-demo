# WebSocket ELK Log Demo (.NET 4.8)

> **⚠️ 離線環境警告**：此專案的 Docker 開發環境處於**斷網 (Air-gapped)** 狀態。所有 Docker 鏡像與 NuGet 套件必須預先在聯網環境下載、打包，再手動搬移至此環境執行。

這個專案示範如何在 Docker 環境中，讓 .NET 4.8 透過 WebSocket 將 Log 傳送到 ELK。詳細的離線部署步驟請參閱 [IMPLEMENTATION_GUIDE.md](./IMPLEMENTATION_GUIDE.md)。

## 架構
- **.NET 4.8 App**: 透過 Mono 在 Docker 執行，作為 WebSocket Client。
- **Node.js Relay**: WebSocket Server，將接收到的資料轉發給 Logstash HTTP Input。
- **ELK Stack**: Logstash -> Elasticsearch -> Kibana。

## 使用步驟 (離線環境)
1. **確認鏡像已載入**：確保已執行 `docker load` 載入 .NET 4.8 與 ELK 相關鏡像。
2. **啟動服務**：`docker-compose up -d`
3. **等待 ELK 啟動**（約 2 分鐘）。
4. **進入 Kibana**：訪問 `http://localhost:5601`（本地連線不受斷網影響）。
5. **建立 Index Pattern**：在 Kibana 中建立 `websocket-logs-*` 即可查看 Log。
