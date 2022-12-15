using System.ComponentModel;
using System.Reflection;

namespace EcommerceMVC.Enums
{
	public enum ProductTypeEnum
	{
		Available = 1,
		[Description("Out of Stock")]
		OutOfStock
	}

	public enum ProductCategoryEnum
	{
		Laptop = 1,
		Phone,
		Shorts,
		Jacket,
		Shirt,
		Trouser,
		SweatPants
	}

	public enum GenderTypeEnum
	{
		Male = 1,
		Female
	}

	public static class Enums
	{
		public static string GetDescription(this Enum GenericEnum)
		{
			Type genericEnumType = GenericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var _Attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				if ((_Attribs != null && _Attribs.Count() > 0))
				{
					return ((DescriptionAttribute)_Attribs.ElementAt(0)).Description;
				}
			}

			return GenericEnum.ToString();
		}
	}
}
