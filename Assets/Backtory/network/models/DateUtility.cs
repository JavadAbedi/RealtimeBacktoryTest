//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
namespace Pegah.SaaS.Models
{
	public class DateUtility
	{
		static readonly DateTime epoch=new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);

		public DateUtility ()
		{
		}

		public static String getCurrentTime() {
			return (long)(DateTime.UtcNow - epoch).TotalMilliseconds + "";
		}
	}
}
