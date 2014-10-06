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
using System.Collections.Generic;

namespace OpenPhotosynth.RichClient
{
    public class Messenger
    {
        public static readonly Messenger Default = new Messenger();

        private Dictionary<string, List<Action<object>>> listeners = new Dictionary<string, List<Action<object>>>();

        private Messenger()
        {
        }

        public void Register(string type, Action<object> callback)
        {
            List<Action<object>> listenersForType;
            if (!this.listeners.TryGetValue(type, out listenersForType))
            {
                listenersForType = new List<Action<object>>();
                this.listeners[type] = listenersForType;
            }

            listenersForType.Add(callback);
        }

        public void Send(string type, object data)
        {
            List<Action<object>> listenersForType;
            if (this.listeners.TryGetValue(type, out listenersForType))
            {
                foreach (var listener in listenersForType)
                {
                    listener(data);
                }
            }
        }
    }
}
