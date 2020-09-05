using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace master_slave_pattern.Services
{
    public class Master
    {
        public string Mannage()
        {
            var results = new List<string>();
            var actions = SplitCommand(new List<string> { "product", "customer", "relation" });

            try
            {
                return CallSlave(actions);
            }
            catch(AggregateException ae)
            {
                foreach(var e in ae.InnerExceptions)
                {
                    results.Add($"{e.Message}");
                }
            }
            return JsonSerializer.Serialize(results);
        }

        public List<Func<string>> SplitCommand(IEnumerable<string> commands)
        {
            List<Func<string>> actions = new List<Func<string>>();
            foreach(var command in commands)
            {
                switch (command)
                {
                    case "product":
                        actions.Add(() => CreateProduct("myproduct"));
                        break;
                    case "customer":
                        actions.Add(() => CreateCustomer("myclustomer"));
                        break;
                    case "relation":
                        actions.Add(() => CreateRelation("myproduct", "myclustomer"));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return actions;
        }

        public string CreateProduct(string name)
        {
            Thread.Sleep(1000);
            return $"Product: {name}";
        }

        public string CreateCustomer(string name)
        {
            Thread.Sleep(2000);
            return $"Customer: {name}";
        }

        public string CreateRelation(string productName, string customerName)
        {
            Thread.Sleep(3000);
            return $"Product: {productName}, Customer: {customerName}";
        }

        public string CallSlave(IEnumerable<Func<string>> tasks)
        {
            List<CancellationToken> ctses = new List<CancellationToken>();
            List<Task<string>> slaves = new List<Task<string>>();
            List<string> results = new List<string>();
            foreach(var task in tasks)
            {
                var cts = new CancellationTokenSource();
                ctses.Add(cts.Token);
                var slave = Task<string>.Factory.StartNew(task, cts.Token);
                slaves.Add(slave);
            }
            var para = CancellationTokenSource.CreateLinkedTokenSource(ctses.ToArray());
            // Create callcel token

            try
            {
                bool cancelled = Task.WaitAll(slaves.ToArray(), 1000, para.Token);
                if (cancelled)
                {
                    para.Cancel();
                }

                foreach(var task in slaves)
                {
                    results.Add($"Id: {task.Id}, Status: {task.Status}, Result:{task.Result}{Environment.NewLine}");
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    if(e is Exception)
                    {
                        // Case insert data fail handle rollback

                        return true;
                    }
                    return false;
                });
            }
            return JsonSerializer.Serialize(results);
        }
    }
}
