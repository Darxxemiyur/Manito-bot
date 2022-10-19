using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.GuideBook
{
	/// <summary>
	/// Represents an object that is linkable via its path
	/// </summary>
	public interface IGlobalLinkable
	{
		string Path => DistrictName + "." + LocalName;
		string DistrictName {
			get;
		}
		string LocalName {
			get;
		}
		string Content {
			get;
		}
		string WholeObject => Path + Content;
		int GetHash();
	}
}
