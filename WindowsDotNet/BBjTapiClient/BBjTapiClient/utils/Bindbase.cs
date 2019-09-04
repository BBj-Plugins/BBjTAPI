/*
 * What is BindBase.cs
 * This class simplyfies the managing of the UI binding notification impulses.
 * Means changing a value of a member of a binded data model object (just a class with 
 * public members) will directly going to be inform the UI and refresh the proper 
 * visual component which binds that public member of such data model class object.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel; // for INotifyPropertyChanged 
using System.Runtime.CompilerServices; // for CallerMemberName

namespace BBjTapiClient.utils
{
        public abstract class Bindbase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]String propertyName = null)
            {
                if (object.Equals(storage, value)) return false;
                storage = value;
                this.OnPropertyChanged(propertyName);
                return true;
            }
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var eventHandler = this.PropertyChanged;
                if (eventHandler != null)
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
}
