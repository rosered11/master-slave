using master_slave_pattern.Enums;
using master_slave_pattern.Infrastructure.Models;
using master_slave_pattern.Serializer;
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
        private Dictionary<TypeServiceEnum, Task<string>> slaves = new Dictionary<TypeServiceEnum, Task<string>>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly LogServices _log;
        private readonly ProductServices _productContext;
        private readonly CustomerServices _customerContext;
        private readonly RelationServices _relationContext;
        public Master(LogServices log, ProductServices productContext, 
            CustomerServices customerContext, RelationServices relationContext)
        {
            _log = log;
            _productContext = productContext;
            _customerContext = customerContext;
            _relationContext = relationContext;
        }
        private void AddStep(string message)
        {
            _number += 1;
            _states.Add(new State { step = _number, message = message });
        }
        public string Mannage(Request request)
        {
            try
            {
                var actions = SplitCommand(request);
                CallSlave(actions);
                return JsonSerializer.Serialize(_states);
            }
            catch(AggregateException ae)
            {
                foreach(var e in ae.InnerExceptions)
                {
                    _log.Create(new Logs(e.Message));
                    AddStep(e.Message);
                }
            }
            catch(Exception ex)
            {
                _log.Create(new Logs(ex.Message));
                AddStep(ex.Message);
            }
            return JsonSerializer.Serialize(_states);
        }

        public Dictionary<TypeServiceEnum, Func<string>> SplitCommand(Request request)
        {
            Dictionary<TypeServiceEnum, Func<string>> actions = new Dictionary<TypeServiceEnum, Func<string>>();

            if(request.ReqProduct != null)
            {
                actions.Add(TypeServiceEnum.Product, () => CreateProduct(request.ReqProduct));
            }

            if(request.ReqProduct != null)
            {
                actions.Add(TypeServiceEnum.Customer, () => CreateCustomer(request.ReqCustomer));
            }

            if(request.Relations != null)
            {
                actions.Add(TypeServiceEnum.Relation, () => CreateRelation(request.Relations));
            }

            return actions;
        }

        public string CreateProduct(ReqProduct reqProduct)
        {
            string message = "Create product success...";
            try
            {
                Thread.Sleep(reqProduct.Timeout);

                foreach (var item in reqProduct.Products)
                {
                    var check = _productContext.Get(item.Name);
                    if(check == null)
                    {
                        _productContext.Create(new Infrastructure.Models.Product { Name = item.Name });
                    }
                    else
                    {
                        _productContext.Update(check.Id, new Infrastructure.Models.Product { Name = item.Name });
                    }
                }
            }
            catch(Exception ex)
            {
                message = $"Fail!! {ex.Message}";
            }
            

            return $"Product: {message}";
        }

        public string CreateCustomer(ReqCustomer reqCustomer)
        {
            string message = "Create customer success..";
            try
            {
                Thread.Sleep(reqCustomer.Timeout);

                foreach (var item in reqCustomer.Customers)
                {
                    var check = _customerContext.Get(item.Name);
                    if(check == null)
                    {
                        _customerContext.Create(new Infrastructure.Models.Customer { Name = item.Name });
                    }
                    else
                    {
                        _customerContext.Update(check.Id, new Infrastructure.Models.Customer { Name = item.Name });
                    }
                }
            }
            catch(Exception ex)
            {
                message = $"Fail!! {ex.Message}";
            }
            
            return $"Customer: {message}";
        }

        public string CreateRelation(IEnumerable<Serializer.Relation> relations)
        {
            string message = "Create relation success...";
            try
            {
                for (int i = 0; i < 1; i++)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                }

                foreach (var item in relations)
                {
                    var productId = _productContext.Get(item.ProductName).Id;
                    var customerId = _customerContext.Get(item.CustomerName).Id;
                    var relationProduct = _relationContext.GetByProductId(productId);
                    var check = relationProduct.FirstOrDefault(x => x.CustomerId == customerId);
                    if (check == null)
                    {
                        _relationContext.Create(new Infrastructure.Models.Relation { ProductId = productId, CustomerId = customerId });
                    }
                    else
                    {
                        _relationContext.Update(check.Id, new Infrastructure.Models.Relation { ProductId = productId, CustomerId = customerId });
                    }
                }
            }
            catch(Exception ex)
            {
                message = $"Fail!! {ex.Message}";
                _cts.Cancel();
            }
           
            return $"Relation: {message}";
        }

        public void CallSlave(Dictionary<TypeServiceEnum, Func<string>> tasks)
        {
            if (tasks.Count == 0)
                return;
            List<CancellationToken> ctses = new List<CancellationToken>();

            foreach (var task in tasks)
            {
                var slave = Task.Factory.StartNew(task.Value, _cts.Token);
                slaves.Add(task.Key, slave);
            }

            #region Optional this function for monitor
            // For this function maybe not need because has the function Task.WaitAll();
            Task.Factory.StartNew(() =>
            {
                bool isCancelled = _cts.Token.WaitHandle.WaitOne();
                if (isCancelled)
                {
                    string message = "Has cancel task.";
                    _log.Create(new Logs(message));
                    AddStep(message);
                }
                else
                {
                    string message = "This transaction {0} is processing...";
                    foreach (var check in slaves)
                    {
                        if (check.Value.Status != TaskStatus.RanToCompletion)
                        {
                            _log.Create(new Logs(string.Format(message, check.Key)));
                            AddStep(string.Format(message, check.Key));
                        }
                    }
                }
            });

            #endregion Optional this function for monitor

            Task.WaitAll(slaves.Values.ToArray());
            foreach(var item in slaves.Values)
            {
                AddStep(item.Result);
                _log.Create(new Logs(item.Result));
            }
            AddStep("Migrate result success...");
        }

        public void Rollback()
        {

        }
    }
}
