﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class OrderStep
	{
		public string Description {
			get; set;
		}
		public OrderStepType Type {
			get;
		}
	}
}