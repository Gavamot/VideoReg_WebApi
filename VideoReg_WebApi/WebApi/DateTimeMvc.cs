using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using WebApi.Services;

namespace WebApi
{
    public class DateTimeMvc
    {
        public static CultureInfo Culture => CultureInfo.InvariantCulture;

        public class DateTimeConverter : JsonConverter<DateTime>
        {
            readonly IDateTimeService dateTimeService;
            public DateTimeConverter(IDateTimeService dateTimeService)
            {
                this.dateTimeService = dateTimeService;
            }
            
            public override DateTime ReadJson(JsonReader reader, Type objectType, [AllowNull] DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var value = reader.ReadAsString();
                return dateTimeService.Parse(value);
            }

            public override void WriteJson(JsonWriter writer, [AllowNull] DateTime value, JsonSerializer serializer)
            {
                var _ = value.ToString(DateTimeService.DefaultMsFormat, Culture);
                writer.WriteValue(_);
            }
        }

        public class UtcAwareDateTimeModelBinder : IModelBinder
        {
            
            private readonly DateTimeStyles _supportedStyles;
            readonly IDateTimeService dateTimeService;
            private readonly DateTimeConverter converter;

            public UtcAwareDateTimeModelBinder(DateTimeStyles supportedStyles, IDateTimeService dateTimeService)
            {
                _supportedStyles = supportedStyles;
                this.dateTimeService = dateTimeService;
                converter = new DateTimeConverter(dateTimeService);
            }

            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException(nameof(bindingContext));
                }

                var modelName = bindingContext.ModelName;
                var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
                if (valueProviderResult == ValueProviderResult.None)
                {
                    // no entry
                    return Task.CompletedTask;
                }

                var modelState = bindingContext.ModelState;
                modelState.SetModelValue(modelName, valueProviderResult);

                var metadata = bindingContext.ModelMetadata;
                var type = metadata.UnderlyingOrModelType;

                var value = valueProviderResult.FirstValue;
                object model = null;
                if (string.IsNullOrWhiteSpace(value))
                {
                    model = null;
                }
                else if (type == typeof(DateTime))
                {
                    model = dateTimeService.Parse(value);
                }
                else
                {
                    // unreachable
                    throw new NotSupportedException();
                }

                // When converting value, a null model may indicate a failed conversion for an otherwise required
                // model (can't set a ValueType to null). This detects if a null model value is acceptable given the
                // current bindingContext. If not, an error is logged.
                if (model == null && !metadata.IsReferenceOrNullableType)
                {
                    modelState.TryAddModelError(
                        modelName,
                        metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                            valueProviderResult.ToString()));
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(model);
                }

                return Task.CompletedTask;
            }
        }

        public class DateTimeModelBinderProvider : IModelBinderProvider
        {
            // You could make this a property to allow customization
            internal static readonly DateTimeStyles SupportedStyles =
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces;

            private readonly IDateTimeService dateTimeService;
            public DateTimeModelBinderProvider(IDateTimeService dateTimeService)
            {
                this.dateTimeService = dateTimeService;
            }

            /// <inheritdoc />
            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var modelType = context.Metadata.UnderlyingOrModelType;
                if (modelType == typeof(DateTime))
                {
                    return new UtcAwareDateTimeModelBinder(SupportedStyles, this.dateTimeService);
                }

                return null;
            }
        }
    }
}