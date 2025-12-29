# WinUtility

一个面向 Windows 的 .NET 8 实用工具库，封装了进程管理、网络适配器、防火墙、注册表、计划任务、服务、窗口控制、系统信息、磁盘与分区、开机启动项、电源管理、命令执行器等常用能力；配套 WinUtilDemo 展示典型用法。

## 项目介绍
- 目标：在纯 .NET 环境下，以统一、易用的 API 操作 Windows 常见系统功能
- 特点：无额外本机依赖；尽量使用系统自带命令（netsh、schtasks、shutdown 等）或 Windows API
- 平台：仅 Windows（库目标框架 net8.0；示例工程目标 net8.0-windows10.0.17763.0）
- 适用场景：企业运维工具、安装器、桌面管理器、自动化脚本宿主等

## 快速开始
### 前置条件
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/) 已安装
- 部分操作需要管理员权限（服务、防火墙、网络、注册表 LocalMachine 等）

### 获取与编译
```bash
dotnet restore
dotnet build
```

### 引用方式
- 作为项目引用（推荐）：在你的应用工程中添加对 WinUtility/WinUtil.csproj 的 ProjectReference
- 作为 DLL 使用：编译后引用 WinUtility 项目的输出 DLL

### 运行示例
```bash
dotnet run -p WinUtilDemo
```
示例包含针对用户/组、进程、计划任务、防火墙、网络、窗口等的演示方法。

## 目录结构
```
WinUtility.sln
├─ WinUtility/                # 核心库（命名空间主要为 WinUtil/WinService）
│  ├─ CommandExecutor.cs      # 通用命令执行器（CMD/PowerShell/外部进程）
│  ├─ DiskManager.cs          # 磁盘/卷/分区信息与空间告警
│  ├─ EnvironmentHelper.cs    # 环境变量读写（WinService 命名空间）
│  ├─ FingerPrint.cs          # 机器指纹生成（MachineFingerprint）
│  ├─ FirewallManager.cs      # 防火墙规则/配置
│  ├─ NetworkAdapter.cs       # 网卡信息与配置（DHCP/静态IP/DNS）
│  ├─ PathHelper.cs           # 常用系统/用户目录路径
│  ├─ PowerManager.cs         # 关机/重启/休眠/锁屏/计划关机
│  ├─ ProcessManager.cs       # 进程查询/启动/结束/CPU采样
│  ├─ RegistryManager.cs      # 注册表操作 + 代理扩展
│  ├─ ScheduledTaskManager.cs # 计划任务增删改查/运行/结束/查询
│  ├─ ServiceManager.cs       # Windows 服务管理 + 常见服务扩展
│  ├─ ShortcutManager.cs      # 桌面/开始菜单快捷方式与任务栏固定
│  ├─ StartupManager.cs       # 开机启动项（Run/RunOnce）
│  ├─ SystemInfoManager.cs    # CPU/内存/主板/BIOS/OS 信息
│  ├─ TempFileCleaner.cs      # 临时文件清理（WinService 命名空间）
│  └─ WindowManager.cs        # 窗口枚举/移动/置顶/最小化/最大化
│
└─ WinUtilDemo/               # 示例工程（控制台/WinForms环境）
   ├─ Program.cs
   ├─ TCommandExecutor.cs
   ├─ TDiskManager.cs
   ├─ TFirewallManager.cs
   ├─ TNetworkAdapter.cs
   ├─ TProcessManager.cs
   ├─ TRegistryManager.cs
   ├─ TServiceManager.cs
   └─ TWindowManager.cs
```

## 使用说明（精选）
- 命令执行
```csharp
var exec = new WinUtil.CommandExecutor();
var r1 = exec.Execute("ipconfig", "/all");          // 外部命令
var r2 = exec.ExecuteCmd("dir /b", Environment.CurrentDirectory);
var r3 = exec.ExecutePowerShell("Get-Date");
```
- 防火墙规则
```csharp
var fm = new WinUtil.FirewallManager();
fm.AddPortRule("Allow-8080", 8080, FirewallManager.Protocol.TCP, FirewallManager.Direction.In, FirewallManager.RuleAction.Allow);
fm.DeleteRule("Allow-8080");
```
- 计划任务
```csharp
var sm = new WinUtil.ScheduledTaskManager();
sm.CreateDaily("MyJob", @"C:\Apps\app.exe", "-arg1", "09:00", highest:true);
sm.Run("MyJob");
sm.Delete("MyJob");
```
- 开机启动项
```csharp
var startup = new WinUtil.StartupManager();
startup.EnableForCurrentUser("MyApp", @"C:\Apps\app.exe", "--silent");
bool enabled = startup.IsEnabledForCurrentUser("MyApp");
startup.DisableForCurrentUser("MyApp");
```
- 用户与组
```csharp
var um = new WinUtil.UserManager();
um.CreateUser("Alice", "P@ssw0rd!", active:true, passwordNeverExpires:true);
um.AddUserToGroup("Users", "Alice");
var users = um.GetUsers();
um.DeleteUser("Alice");
```

## API 文档（概览）
以下为主要类型与方法的快速索引，详细参数/返回值请参阅源码注释与示例。

### WinUtil.CommandExecutor
- 属性：DefaultTimeout、DefaultWorkingDirectory、DefaultEnvironmentVariables
- 方法：
```csharp
CommandResult Execute(string command, string? arguments = null, string? workingDirectory = null, int? timeout = null, Dictionary<string,string>? env = null);
Task<CommandResult> ExecuteAsync(string command, string? arguments = null, string? workingDirectory = null, int? timeout = null, Dictionary<string,string>? env = null, CancellationToken ct = default);
CommandResult ExecutePowerShell(string script, string? workingDirectory = null, int? timeout = null);
CommandResult ExecuteCmd(string command, string? workingDirectory = null, int? timeout = null, Dictionary<string,string>? env = null);
```
```csharp
public class CommandResult { int ExitCode; string Output; string Error; bool IsSuccess; bool IsTimeout; long ElapsedMilliseconds; }
```

### WinUtil.ProcessManager
- 类型：ProcessInfo(Id, Name, MainWindowTitle, FileName, StartTime, WorkingSetBytes, CpuPercent)
- 方法：
```csharp
IReadOnlyList<ProcessInfo> GetAllProcesses();
IReadOnlyList<ProcessInfo> GetProcessesByName(string processName);
ProcessInfo? GetProcessById(int pid);
ProcessInfo StartProcess(string fileName, string? arguments = null, string? workingDirectory = null, bool createNoWindow = false, bool useShellExecute = true);
bool KillProcessById(int pid, bool force = true);
int  KillProcessesByName(string processName, bool force = true);
string FormatProcessList(IEnumerable<ProcessInfo> processes);
IReadOnlyList<ProcessInfo> GetTopByMemory(int topN = 10);
Task<IReadOnlyList<ProcessInfo>> GetTopByCpuAsync(int topN = 10, int sampleMilliseconds = 1000, CancellationToken ct = default);
static string FindProcessByPortWithNetstat(int port);
```

### WinUtil.NetworkAdapter
- 类型：AdapterInfo(Name, Description, MacAddress, Enabled, Type, Status, IPv4Addresses, IPv6Addresses, Gateways, DnsServers)
- 方法：
```csharp
IReadOnlyList<AdapterInfo> GetAdapters();
AdapterInfo? GetAdapter(string name);
bool Enable(string name); bool Disable(string name);
bool SetDhcp(string name);
bool SetStaticIp(string name, string ip, string mask, string? gateway = null, int gwMetric = 1);
bool SetDnsDhcp(string name);
bool SetDnsServers(string name, IEnumerable<string> servers);
```

### WinUtil.FirewallManager
- 枚举：Direction(In/Out)、RuleAction(Allow/Block)、Protocol(Any/TCP/UDP)
- 方法：
```csharp
bool EnableAllProfiles(); bool DisableAllProfiles();
bool EnableProfile(string profile); bool DisableProfile(string profile);
string ShowStatus(); string ShowRules(string? name = null);
bool AddPortRule(string name, int port, Protocol protocol, Direction direction, RuleAction action, string? profile = null);
bool AddProgramRule(string name, string programPath, Direction direction, RuleAction action, string? profile = null);
bool DeleteRule(string name, Direction? direction = null);
bool OpenPort(string name, int port, Protocol protocol = Protocol.TCP, string? profile = null);
bool ClosePort(string name, int port, Direction? direction = null);
```

### WinUtil.RegistryManager
- 枚举：RootHive(CurrentUser/LocalMachine/ClassesRoot/Users)
- 子键/键值操作：
```csharp
RegistryKey CreateOrOpenSubKey(RootHive root, string subKey);
bool SubKeyExists(RootHive root, string subKey);
bool DeleteSubKey(RootHive root, string subKey, bool recursive = true);
bool DeleteValue(RootHive root, string subKey, string valueName);
bool ValueExists(RootHive root, string subKey, string valueName);
object? GetValue(RootHive root, string subKey, string valueName);
void SetString(RootHive root, string subKey, string valueName, string? value);
string? GetString(RootHive root, string subKey, string valueName, string? defaultValue = null);
void SetInt(RootHive root, string subKey, string valueName, int value);
int GetInt(RootHive root, string subKey, string valueName, int defaultValue = 0);
void SetBool(RootHive root, string subKey, string valueName, bool value);
bool GetBool(RootHive root, string subKey, string valueName, bool defaultValue = false);
```
- 扩展（代理设置）：
```csharp
void SwitchIEProxy(this RegistryManager reg, bool enabled);
void SetProxyServer(this RegistryManager reg, string ip, ushort port);
```

### WinUtil.StartupManager
```csharp
void EnableForCurrentUser(string appName, string executablePath, string? arguments = null, bool runOnce = false);
void DisableForCurrentUser(string appName, bool runOnce = false);
bool IsEnabledForCurrentUser(string appName, bool runOnce = false);
void EnableForAllUsers(string appName, string executablePath, string? arguments = null, bool runOnce = false);
void DisableForAllUsers(string appName, bool runOnce = false);
bool IsEnabledForAllUsers(string appName, bool runOnce = false);
IReadOnlyDictionary<string, string> ListCurrentUser(bool runOnce = false);
IReadOnlyDictionary<string, string> ListAllUsers(bool runOnce = false);
```

### WinUtil.ScheduledTaskManager
```csharp
bool CreateDaily(string name, string executablePath, string? arguments, string startTime, string? startDate = null, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true);
bool CreateWeekly(string name, string executablePath, string? arguments, string startTime, string days, string? startDate = null, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true);
bool CreateOnce(string name, string executablePath, string? arguments, string startTime, string startDate, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true);
bool CreateAtLogon(string name, string executablePath, string? arguments, string? runUser = null, string? runPassword = null, bool highest = true, bool force = true);
bool ModifyCommand(string name, string executablePath, string? arguments);
bool ModifyStartTime(string name, string startTime);
bool Enable(string name); bool Disable(string name);
bool Delete(string name); bool Run(string name); bool End(string name);
string Query(string? name = null, bool verbose = true);
```

### WinUtil.ServiceManager 与扩展
```csharp
ServiceControllerStatus? GetStatus(string serviceName);
bool Exists(string serviceName);
void Start(string serviceName, TimeSpan? timeout = null);
void Stop(string serviceName, TimeSpan? timeout = null);
void Restart(string serviceName, TimeSpan? timeoutPerStep = null);
Task RestartAsync(string serviceName, TimeSpan? timeoutPerStep = null, CancellationToken ct = default);
void WaitForStatus(string serviceName, ServiceControllerStatus desired, TimeSpan? timeout = null);
```
- 常见服务扩展（ServiceExtensions）：
```csharp
void RestartPrintSpooler(this ServiceManager sc);
void StartWindowsUpdate(this ServiceManager sc);
void StopWindowsUpdate(this ServiceManager sc);
void ResyncWindowsTime(this ServiceManager sc);
void RestartDnsClient(this ServiceManager sc);
void StartFirewall(this ServiceManager sc);
void StopFirewall(this ServiceManager sc);
```

### WinUtil.WindowManager
- 类型：WindowInfo(Handle, Title, ClassName, ProcessId, X, Y, Width, Height, Visible, TopMost)
- 方法：
```csharp
IReadOnlyList<WindowInfo> EnumerateWindows(bool onlyVisible = true);
WindowInfo? GetWindowInfo(IntPtr hWnd);
bool SetTopMost(IntPtr hWnd, bool topMost);
bool Minimize(IntPtr hWnd); bool Maximize(IntPtr hWnd); bool Restore(IntPtr hWnd); bool Activate(IntPtr hWnd);
bool Move(IntPtr hWnd, int x, int y);
bool Resize(IntPtr hWnd, int width, int height);
bool MoveResize(IntPtr hWnd, int x, int y, int width, int height);
```

### WinUtil.PowerManager
```csharp
bool Shutdown(int seconds = 0, bool force = false);
bool Restart(int seconds = 0, bool force = false);
bool Logoff();
bool Hibernate(bool ensureEnabled = true);
bool EnableHibernate(); bool DisableHibernate();
bool LockScreen();
bool ScheduleShutdown(DateTime when, bool restart = false, bool force = false);
bool ScheduleShutdownAfterSeconds(int seconds, bool restart = false, bool force = false);
bool CancelScheduled();
```

### WinUtil.SystemInfoManager
- 类型：SystemInfo(CpuModel, LogicalProcessorCount, TotalPhysicalMemoryBytes, MotherboardManufacturer, MotherboardProduct, BiosVersion, OsProductName, OsBuild, OsVersionString, ComputerName)
- 方法：
```csharp
SystemInfo GetSystemInfo();
```

### WinUtil.DiskManager
- 类型：VolumeInfo、PartitionInfo、DiskInfo
- 方法：
```csharp
IReadOnlyList<VolumeInfo> GetVolumes();
IReadOnlyList<PartitionInfo> GetPartitions();
IReadOnlyList<DiskInfo> GetDisks();
IReadOnlyList<VolumeInfo> ListLowSpace(double thresholdPercent = 10.0, long minFreeBytes = 5L * 1024 * 1024 * 1024);
```

### WinUtil.ShortcutManager
```csharp
string CreateShortcut(string shortcutPath, string targetPath, string? arguments = null, string? workingDirectory = null, string? iconPath = null, string? description = null);
bool DeleteShortcut(string shortcutPath);
string CreateDesktopShortcut(string name, string targetPath, string? arguments = null, string? iconPath = null);
bool DeleteDesktopShortcut(string name);
string CreateStartMenuShortcut(string name, string targetPath, bool allUsers = false, string? arguments = null, string? iconPath = null);
bool DeleteStartMenuShortcut(string name, bool allUsers = false);
bool PinToTaskbar(string shortcutPath); bool UnpinFromTaskbar(string shortcutPath);
```

### WinUtil.PathHelper
```csharp
string System32(); string Windows();
string ProgramFiles(); string ProgramFilesX86(); string ProgramData();
string AppDataRoaming(); string AppDataLocal(); string UserProfile();
string Desktop(); string Documents(); string Downloads(); string Pictures(); string Music(); string Videos();
string StartupCurrentUser(); string StartMenuCurrentUser(); string StartupAllUsers(); string StartMenuAllUsers();
string PublicDesktop(); string PublicDocuments();
string Temp(); string OneDrive(); string CurrentAppDirectory();
IReadOnlyDictionary<string,string> CommonMap();
```

### MachineFingerprint（WinUtil.MachineFingerprint）
```csharp
string GetFingerprint();
string Md5(string input);
```

### WinService.EnvironmentHelper（命名空间 WinService）
```csharp
string? Get(string name, EnvScope scope = EnvScope.User);
string GetOrDefault(string name, string defaultValue, EnvScope scope = EnvScope.User);
void Set(string name, string value, EnvScope scope = EnvScope.User);
void Remove(string name, EnvScope scope = EnvScope.User);
enum EnvScope { Process, User, Machine }
```

### WinService.TempFileCleaner（命名空间 WinService）
```csharp
void CleanTempFiles();
void CleanDirectory(string path);
void CleanSpecificFiles(string directoryPath, string filePattern);
```

## 权限与注意事项
- 需要管理员权限的操作：服务控制、防火墙规则、网络适配器配置、注册表 HKEY_LOCAL_MACHINE、部分计划任务
- PowerShell 执行策略：库使用 `-ExecutionPolicy Bypass` 调用；企业环境可根据策略调整
- 某些 API 在杀毒/终端防护软件拦截下可能失败，请在受控环境中测试

## 贡献指南
- 提交 Issue：明确复现步骤/期望行为/环境信息（Windows 版本、.NET SDK 版本）
- 提交 PR：
  - 保持代码风格与现有一致（C# 10/11，nullable 开启）
  - 避免引入不必要的第三方依赖
  - 为新功能添加示例或最小可验证用例
  - 提交信息清晰、原子化（feat/fix/docs/chore 等）
- 分支策略：建议以 feature/xxx 或 fix/xxx 命名主题分支

## 许可
- 本项目采用仓库中的 LICENSE.txt

## 致谢
- 感谢 .NET / Windows API 与系统内置工具（netsh、schtasks、w32tm、shutdown、powercfg 等）
