using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tizen.Security;

namespace TizenSensor.lib
{
	public class SensorPermissionGetter
	{
		protected const string HealthInfoPermissionString = "http://tizen.org/privilege/healthinfo";

		public static void GetPermission(Action<bool> callback)
		{
			var initPermission = PrivacyPrivilegeManager.CheckPermission(HealthInfoPermissionString);
			if (initPermission == CheckResult.Allow)
			{
				callback(true);
				return;
			}
			if (initPermission == CheckResult.Deny)
			{
				callback(false);
				return;
			}

			if (!PrivacyPrivilegeManager.GetResponseContext(HealthInfoPermissionString).TryGetTarget(out var context))
			{
				callback(false);
				return;
			}

			context.ResponseFetched += (sender, e) => callback(e.result == RequestResult.AllowForever);
			PrivacyPrivilegeManager.RequestPermission(HealthInfoPermissionString);
		}
	}
}
