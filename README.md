# GHLearning-EasyHangfire
[![GitHub Actions GHLearning-EasyHangfire](https://github.com/gordon-hung/GHLearning-EasyHangfire/actions/workflows/dotnet.yml/badge.svg)](https://github.com/gordon-hung/GHLearning-EasyHangfire/actions/workflows/dotnet.yml) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/gordon-hung/GHLearning-EasyHangfire)

**Hangfire** 是一個基於 .NET 平台的開源框架，主要用來處理背景任務。它允許開發者將異步或定時執行的工作從主程式中分離出來，讓它們在背景中獨立執行。Hangfire 支援將任務排程並執行，並且具有任務重試、錯誤處理等功能。它的使用相對簡單，並且支持分佈式系統，能夠很好地適應大規模應用場景。

### Hangfire 的核心特點：
1. **背景任務處理**：能夠處理長時間執行或需要延遲執行的任務（如發送電子郵件、處理圖像等），並讓它們在背景中執行，而不會阻塞主應用的流程。
   
2. **任務排程**：支持定時執行任務，例如每隔幾分鐘、每天、每週等。也可以設置特定的時間點執行某些操作。

3. **持久化任務**：Hangfire 會將任務的狀態保存在資料庫中，即使服務重啟或應用程式崩潰，任務也不會丟失，並且會從資料庫中恢復。

4. **重試機制**：當任務執行失敗時，Hangfire 可以根據設置自動重試任務，避免一次失敗導致整個流程的中斷。

5. **儀表板（Dashboard）**：Hangfire 提供了一個基於 Web 的儀表板，可以用來查看任務的狀態、結果、錯誤以及任務的執行情況，這對於監控背景任務非常有幫助。

### 使用契機
1. **異步處理與背景工作**：當你的應用需要處理一些耗時的工作（例如發送大量電子郵件、處理文件、生成報告等），這些工作不應該影響用戶的正常操作，這時就可以使用 Hangfire 將這些工作放到背景中執行，避免阻塞主線程。

2. **定時任務**：如果你的應用需要定期執行某些任務（例如每小時執行一次數據清理、每日生成報表等），Hangfire 可以輕鬆地進行定時排程，並且支持靈活的 Cron 表達式設定。

3. **分佈式系統**：在分佈式環境中，Hangfire 具有內建的支援，能夠在多個服務實例之間協調執行背景任務，並且保證任務不會重複執行。

4. **可靠的任務處理**：如果你的任務需要保證執行的可靠性和持久性，Hangfire 提供了任務持久化和錯誤處理的機制，當任務執行失敗時會自動進行重試，並且可以在儀表板中查看詳細的執行結果。

5. **無需手動管理佇列**：Hangfire 提供了內建的任務佇列系統，你不需要手動管理佇列或使用其他外部工具來處理背景任務。

### 如何使用 Hangfire
1. **安裝 Hangfire 套件**：
   你可以透過 NuGet 安裝 Hangfire：
   ```
   Install-Package Hangfire
   ```

2. **設定 Hangfire**：
   在你的 .NET 應用程式中，通常需要在 `Startup.cs` 中進行配置：
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       // 使用內建的資料庫來儲存任務
       services.AddHangfire(x => x.UseSqlServerStorage("your_connection_string"));

       // 註冊 Hangfire 服務
       services.AddHangfireServer();
   }

   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
   {
       // 設定 Hangfire 儀表板
       app.UseHangfireDashboard();

       // 啟動 Hangfire 伺服器來處理背景任務
       app.UseHangfireServer();
   }
   ```

3. **創建背景任務**：
   你可以在應用程式的任意地方，根據需要創建並排程背景任務：
   ```csharp
   public class MyService
   {
       public void SomeTask()
       {
           // 這個方法會在背景中執行
           Console.WriteLine("背景任務執行中...");
       }
   }

   // 排程背景任務
   BackgroundJob.Enqueue(() => new MyService().SomeTask());
   ```

4. **設定定時任務**：
   你也可以設定定時任務（例如，每天某個時間執行）：
   ```csharp
   RecurringJob.AddOrUpdate(() => new MyService().SomeTask(), Cron.Daily);
   ```

### 結論
Hangfire 是一個非常適合用於處理背景任務、定時任務和異步處理的框架。它提供了簡單的 API 和強大的功能，能夠幫助開發者高效地管理長時間執行的任務，並且不會阻塞主應用程式的執行。如果你的應用程式中有需要定期執行的任務或背景任務，Hangfire 是一個非常好的選擇。