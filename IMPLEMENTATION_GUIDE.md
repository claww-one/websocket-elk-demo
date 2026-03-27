# WebSocket ELK Demo 實作手冊 (含 .NET Framework 4.8 容器化)

> **⚠️ 離線環境警告**：此專案的 Docker 開發環境處於**斷網 (Air-gapped)** 狀態。所有 Docker 鏡像 (Images) 與 NuGet 套件必須預先在聯網環境下載、打包，再手動搬移至此環境執行。

本手冊將指引你如何將一個基於 **.NET Framework 4.8** 的 WebSocket 應用程式與 **ELK Stack (Elasticsearch, Logstash, Kibana)** 整合。由於 .NET Framework 4.8 必須運行在 Windows 容器中，請確保你的 Docker 環境已切換至 **Windows Containers** 模式。

---

## 1. 專案結構說明
```text
websocket-elk-demo/
├── Dockerfile           # 定義 .NET 4.8 應用的 Windows 容器鏡像
├── Program.cs           # WebSocket 伺服器邏輯
├── docker-compose.yml   # 協調應用程式與 ELK 容器
├── logstash.conf        # Logstash 接收規則
└── relay/               # Node.js 轉發層 (可選，用於協議轉換)
```

---

## 2. 實作步驟：.NET Framework 4.8 專案容器化 (離線模式)

### A. 準備 Docker 鏡像 (聯網環境)
在離線環境中，你無法直接從 Docker Hub 拉取鏡像。請在聯網機器上執行以下操作：

1. **拉取基礎鏡像**：
   ```powershell
   docker pull mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022
   ```
2. **匯出鏡像**：
   ```powershell
   docker save -o dotnet-framework-48.tar mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022
   ```
3. **將 `.tar` 檔案搬移至離線環境並載入**：
   ```powershell
   docker load -i dotnet-framework-48.tar
   ```

### B. 撰寫 Dockerfile
針對 .NET 4.8，使用已載入的基礎鏡像。

```dockerfile
# 使用已載入的基礎鏡像
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# 設定工作目錄
WORKDIR /app

# 複製預先編譯好的二進制檔案 (離線環境不建議在 Docker 內執行 NuGet Restore)
COPY ./bin/Release/ .

# 開放 WebSocket 埠位
EXPOSE 8080

# 啟動應用程式
ENTRYPOINT ["YourWebSocketApp.exe"]
```

### C. 編譯專案 (離線編譯)
在離線環境中，請確保所有 NuGet 依賴已完整下載至本地路徑，或在聯網機器編譯後將 `bin/Release` 整體打包搬移過來。

---

## 3. 實作步驟：ELK Stack 整合 (離線配置)

### A. 準備 ELK 鏡像 (聯網環境)
比照 2-A 步驟，將 ELK 相關鏡像預先存為 `.tar` 檔案：
- `docker.elastic.co/elasticsearch/elasticsearch:7.17.0`
- `docker.elastic.co/logstash/logstash:7.17.0`
- `docker.elastic.co/kibana/kibana:7.17.0`

### B. 配置 Logstash (logstash.conf)
Logstash 負責接收來自 WebSocket 應用的日誌並傳送至 Elasticsearch。

```conf
input {
  tcp {
    port => 5044
    codec => json
  }
}

output {
  elasticsearch {
    hosts => ["elasticsearch:9200"]
    index => "websocket-logs-%{+YYYY.MM.dd}"
  }
}
```

### C. 配置 Docker Compose (禁用自動拉取)
確保 `image` 標籤與你 `docker load` 進來的名稱完全一致。

```yaml
version: '3'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.17.0
    environment:
      - discovery.type=single-node
    ports:
      - "9200:9200"

  logstash:
    image: docker.elastic.co/logstash/logstash:7.17.0
    volumes:
      - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf
    ports:
      - "5044:5044"
    depends_on:
      - elasticsearch

  kibana:
    image: docker.elastic.co/kibana/kibana:7.17.0
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch

  websocket-app:
    build: .
    depends_on:
      - logstash
```

---

## 4. 關鍵技術：如何在 .NET 4.8 中傳送日誌
建議使用 **NLog** 或 **Serilog** 的 TCP Target 直接傳送到 Logstash 的 5044 埠位。

**範例代碼 (C#):**
```csharp
var logstashTarget = new NetworkTarget {
    Address = "tcp://logstash:5044",
    Layout = new JsonLayout() // 確保格式為 JSON
};
```

---

## 5. 啟動與測試 (離線環境)
1. **確認所有鏡像已載入**：執行 `docker images` 查看。
2. **執行容器**：
   ```bash
   docker-compose up -d
   ```
3. **存取 Kibana**：打開瀏覽器訪問 `http://localhost:5601`（本地連線不受斷網影響）。
4. **建立 Index Pattern**：在 Kibana 中新增 `websocket-logs-*` 即可看到即時日誌。

---

## ⚠️ 注意事項
- **鏡像更新**：任何版本更新都需要在聯網機器重複 `save`/`load` 流程。
- **NuGet 依賴**：強烈建議在聯網機器完成編譯與打包（Self-contained），避免離線環境因缺套件無法編譯。
- **OS 限制**：.NET Framework 4.8 容器**無法**在 Linux Host 上直接運行。

如有需要針對特定離線傳輸流程（如 USB/私有 Registry）優化的範例，請隨時告知。
