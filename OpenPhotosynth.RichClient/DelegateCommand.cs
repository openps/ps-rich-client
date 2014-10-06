// The MIT License (MIT)

// Copyright (c) 2014 Alec Siu, Eric Stollnitz

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Windows.Input;

namespace OpenPhotosynth.RichClient
{
    public sealed class DelegateCommand : ICommand
    {
        private Action executeAction;
        private Func<bool> canExecuteFunc;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action executeAction, Func<bool> canExecuteFunc = null)
        {
            this.executeAction = executeAction;
            this.canExecuteFunc = canExecuteFunc;
        }

        public void Invalidate()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteFunc != null ? this.canExecuteFunc() : true;
        }

        public void Execute(object parameter)
        {
            if (this.executeAction != null)
            {
                this.executeAction();
            }
        }
    }

    public sealed class DelegateCommand<T> : ICommand
    {
        private Action<T> executeAction;
        private Func<T, bool> canExecuteFunc;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteFunc = null)
        {
            this.executeAction = executeAction;
            this.canExecuteFunc = canExecuteFunc;
        }

        public void Invalidate()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteFunc != null ? this.canExecuteFunc((T)parameter) : true;
        }

        public void Execute(object parameter)
        {
            if (this.executeAction != null)
            {
                this.executeAction((T)parameter);
            }
        }
    }
}
