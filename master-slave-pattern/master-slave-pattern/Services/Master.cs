using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace master_slave_pattern.Services
{
    public class State
    {
        public int step { get; set; }
        public string message { get; set; }
    }
    public class Master
    {
        private List<State> _states = new List<State>();
        private int _number = 0;
        List<Task<string>> slaves = new List<Task<string>>();

        private void AddStep(string message)
        {
            _number += 1;
            _states.Add(new State { step = _number, message = message });
        }
        public string Mannage()
        {
            var results = new List<string>();
            var actions = SplitCommand(new List<string> { "product", "customer", "relation" });

            try
            {
                var empty = CallSlave(actions);
                Thread.Sleep(3000);
                return JsonSerializer.Serialize(_states);
            }
            catch(AggregateException ae)
            {
                foreach(var e in ae.InnerExceptions)
                {
                    AddStep(e.Message);
                    //results.Add($"{e.Message}");
                }
            }
            catch(Exception ex)
            {
                AddStep(ex.Message);
            }
            return JsonSerializer.Serialize(_states);
        }

        public Dictionary<Func<string>, CancellationTokenSource> SplitCommand(IEnumerable<string> commands)
        {
            Dictionary<Func<string>, CancellationTokenSource> actions = new Dictionary<Func<string>, CancellationTokenSource>();
            CancellationTokenSource cts = new CancellationTokenSource();
            foreach (var command in commands)
            {
                switch (command)
                {
                    case "product":
                        cts = new CancellationTokenSource();
                        actions.Add(() => CreateProduct("myproduct", cts.Token), cts);
                        //cts.Cancel();
                        break;
                    case "customer":
                        actions.Add(() => CreateCustomer("myclustomer", cts.Token), cts);
                        break;
                    case "relation":
                        actions.Add(() => CreateRelation("myproduct", "myclustomer", cts.Token), cts);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return actions;
        }

        public string CreateProduct(string name, CancellationToken token)
        {
            // 1. Not work
            Thread.Sleep(1000);

            // 2. Not work
            //bool cancel = token.WaitHandle.WaitOne(1000);
            //if (cancel)
            //{
            //    return $"Canceled Product: {name}";
            //}

            // 3.
            //for (int i = 0; i < 1; i++)
            //{
            //    //token.ThrowIfCancellationRequested();
            //    Thread.Sleep(1000);
            //}
            return $"{Task.CurrentId}{name}";
        }

        public string CreateCustomer(string name, CancellationToken token)
        {
            Thread.Sleep(2000);

            //bool cancel = token.WaitHandle.WaitOne(2000);
            //if (cancel)
            //{
            //    return $"Canceled Product: {name}";
            //}

            //for (int i = 0; i < 2; i++)
            //{
            //    //token.ThrowIfCancellationRequested();
            //    Thread.Sleep(1000);
            //}
            throw new InvalidOperationException("Can't do this! slave1") { Source = "slave1" };
            return $"{Task.CurrentId}{name}";
        }

        public string CreateRelation(string productName, string customerName, CancellationToken token)
        {
            Thread.Sleep(3000);

            //bool cancel = token.WaitHandle.WaitOne(2000);
            //if (cancel)
            //{
            //    return $"Canceled both";
            //}

            //for (int i = 0; i < 3; i++)
            //{
            //    //token.ThrowIfCancellationRequested();
            //    Thread.Sleep(1000);
            //}
            throw new Exception("Can't do this! slave2") { Source = "slave2" };
            return $"{Task.CurrentId}{productName}-{customerName}";
        }

        public string CallSlave(Dictionary<Func<string>, CancellationTokenSource> tasks)
        {
            List<CancellationToken> ctses = new List<CancellationToken>();

            //List<string> results = new List<string>();

            var cts = new CancellationTokenSource();
            ctses.Add(cts.Token);

            foreach (var item in tasks)
            {
                ctses.Add(item.Value.Token);
                var slave = Task<string>.Factory.StartNew(item.Key, cts.Token);
                slaves.Add(slave);
            }
            
            //var para = CancellationTokenSource.CreateLinkedTokenSource(ctses.ToArray());

            try
            {
                Task.WaitAll(slaves.ToArray(), cts.Token);
                return "";
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    //if(e is InvalidOperationException)
                    //{
                    //    // handle case and ignore exception
                    //    cts.Cancel();
                    //    return true;
                    //}
                    return false;
                });
                return "";
            }
            //return JsonSerializer.Serialize(results);
        }
    }
}
