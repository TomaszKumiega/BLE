using System;
using System.Collections.Generic;
using System.Text;
using static Xamarin.Essentials.Permissions;

namespace ERGBLE.Exceptions
{
    public class PermissionNotGranted : Exception
    {
        public Type PermissionType { get; }

        public PermissionNotGranted(Type permissionType)
        {
            PermissionType = permissionType;
        }
    }
}
