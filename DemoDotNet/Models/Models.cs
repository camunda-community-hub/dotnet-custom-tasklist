using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoDotNet.Models
{
    public class Models
    {

        public class VariableInput {
            public string name { get; set; }
            public object value { get; set; }
        }

        public class TaskListResponse
        {
            public List<Task> tasks { get; set; }
        };

        public class GetTaskResponse
        {
            public Task task { get; set; }
        };

        public class ClaimMutationResponse
        {
            public Task claimTask { get; set; }
        };

        public class CompleteMutationResponse
        {
            public Task completeTask { get; set; }
        };


        public class UnClaimMutationResponse
        {
            public Task unClaimTask { get; set; }
        };

        public class Task
        {
        
            public string id { get; set; }
            public string creationTime { get; set; }
            public string assignee { get; set; }
            public string name { get; set; }
            public string processName { get; set; }
            public string candidateGroups { get; set; }
            public string processDefinitionId { get; set; }
            public string formKey { get; set; }
            public string taskState { get; set; }
            public List<Variable> variables { get; set; }

            public override string ToString()
            {
                return GetType().GetProperties()
                    .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                    .Aggregate(
                        new StringBuilder(),
                        (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                        sb => sb.ToString());
            }
        }

        public class Variable
        {
            public string id { get; set; }
            public string name { get; set; }
            public string value { get; set; }
            public string previewValue { get; set; }
            public bool isValueTruncated { get; set; }

            public override string ToString()
            {
                return GetType().GetProperties()
                    .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                    .Aggregate(
                        new StringBuilder(),
                        (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                        sb => sb.ToString());
            }
        }



    }
}

