# WebSocket ELK Demo 實作手冊 (含 .NET Framework 4.8 容器化)

本手冊將指引你如何將一個基於 **.NET Framework 4.8** 的 WebSocket 應用程式與 **ELK Stack (Elasticsearch, Logstash, Kibana)** 整合。由於 .NET Framework 4.8 必須運行在 Windows 容器中，請確保你的 Docker 環境已切換至 **Windows Containers** 模式（若在 Linux 環境，則需使用 .NET 6+ 替代）。

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

## 2. 實作步驟：.NET Framework 4.8 專案容器化

### A. 撰寫 Dockerfile
針對 .NET 4.8，我們使用 Microsoft 提供的官方 ASP.NET 鏡像作為基礎。

```dockerfile
# 使用 Windows Server Core 搭配 .NET 4.8 執行環境
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# 設定工作目錄
WORKDIR /app

# 複製編譯好的二進制檔案 (假設在 bin/Release 目錄)
COPY ./bin/Release/ .

# 開放 WebSocket 埠位
EXPOSE 8080

# 啟動應用程式
ENTRYPOINT ["YourWebSocketApp.exe"]
```

### B. 編譯專案
在你的開發環境（VS 2022 或 MSBuild）中執行：
```powershell
msbuild /p:Configuration=Release
```

---

## 3. 實作步驟：ELK Stack 整合

### A. 配置 Logstash (logstash.conf)
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

### B. 配置 Docker Compose
這會同時啟動應用與 ELK 全家桶。

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

## 5. 啟動與測試
1. **執行容器**：
   ```bash
   docker-compose up -d
   ```
2. **存取 Kibana**：打開瀏覽器訪問 `http://localhost:5601`。
3. **建立 Index Pattern**：在 Kibana 中新增 `websocket-logs-*` 即可看到即時日誌。

---

## ⚠️ 注意事項
- **OS 限制**：.NET Framework 4.8 容器**無法**在 Linux Host 上直接運行。
- **替代方案**：若必須在 Linux 上運行，強烈建議將專案升級至 **.NET 8**，其 Dockerfile 將精簡 80% 且支援跨平台。

如有需要升級至 .NET 8 的範例，請隨時告知。
