using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrancingTurtle.Helpers.ModelBinder
{
    public class TrimModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult?.AttemptedValue == null)
            {
                return null;
            }

            if (valueResult.AttemptedValue == string.Empty)
            {
                return string.Empty;
            }

            return valueResult.AttemptedValue.Trim();
        }
    }
}