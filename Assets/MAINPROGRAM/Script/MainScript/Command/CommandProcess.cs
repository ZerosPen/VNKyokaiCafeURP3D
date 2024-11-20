using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Commands
{
    public class CommandProcess
    {
        public Guid Id;
        public string processName;
        public Delegate command;
        public CoroutineWrapper runningProcess;
        public string[] args;

        public UnityEvent OnTerminateAction;

        public CommandProcess(Guid iD, string processName, Delegate command, CoroutineWrapper runningProcess, string[] args, UnityEvent onTerminateAction = null)
        {
            Id = iD;
            this.processName = processName;
            this.command = command;
            this.runningProcess = runningProcess;
            this.args = args;
            this.OnTerminateAction = onTerminateAction;
        }
    }
}