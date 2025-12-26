using System;
using System.Diagnostics;
using WinUtility;

namespace TWinService
{
    public class TFirewallManager
    {
        public static void Test()
        {
            var fm = new FirewallManager();
            Console.WriteLine(fm.ShowStatus());

            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "";
            if (string.IsNullOrWhiteSpace(exe)) return;

            string ruleProg = "Demo-Allow-Program";
            string rulePort = "Demo-Allow-Port-8080";

            var ok1 = fm.AddProgramRule(ruleProg, exe, FirewallManager.Direction.In, FirewallManager.RuleAction.Allow);
            Console.WriteLine($"AddProgramRule: {ok1}");

            var ok2 = fm.AddPortRule(rulePort, 8080, FirewallManager.Protocol.TCP, FirewallManager.Direction.In, FirewallManager.RuleAction.Allow);
            Console.WriteLine($"AddPortRule: {ok2}");

            Console.WriteLine(fm.ShowRules(ruleProg));
            Console.WriteLine(fm.ShowRules(rulePort));

            var del1 = fm.DeleteRule(ruleProg);
            var del2 = fm.DeleteRule(rulePort);
            Console.WriteLine($"DeleteProgramRule: {del1}, DeletePortRule: {del2}");
        }
    }
}
