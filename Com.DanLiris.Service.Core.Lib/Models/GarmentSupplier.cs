﻿using Com.DanLiris.Service.Core.Lib.Helpers;
using Com.DanLiris.Service.Core.Lib.Services;
using Com.Moonlay.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Core.Lib.Models
{
    public class GarmentSupplier : StandardEntity, IValidatableObject
	{
		[MaxLength(255)]
		public string UId { get; set; }

		[StringLength(100)]
		public string Code { get; set; }

		[StringLength(500)]
		public string Name { get; set; }

		[StringLength(3000)]
		public string Address { get; set; }

		[StringLength(500)]
		public string Country { get; set; }

		[StringLength(500)]
		public string Contact { get; set; }

		[StringLength(500)]
		public string PIC { get; set; }

		public bool? Import { get; set; }
		public bool? UseVat { get; set; }
		[StringLength(100)]
		public string NPWP { get; set; }

		[StringLength(500)]
		public string SerialNumber { get; set; }

		[StringLength(500)]
		public string Description { get; set; }

		public bool? UseTax { get; set; }

		/* IncomeTaxes*/
		public int IncomeTaxesId { get; set; }
		public string IncomeTaxesName { get; set; }
		public double? IncomeTaxesRate { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			List<ValidationResult> validationResult = new List<ValidationResult>();

			if (string.IsNullOrWhiteSpace(this.Code))
				validationResult.Add(new ValidationResult("Code is required", new List<string> { "code" }));

			if (string.IsNullOrWhiteSpace(this.Name))
				validationResult.Add(new ValidationResult("Name is required", new List<string> { "name" }));

			//if (UseTax==true && string.IsNullOrWhiteSpace(IncomeTaxesName))
			//	validationResult.Add(new ValidationResult("PPH is required", new List<string> { "incometax" }));

			if (this.Import.Equals(null))
				this.Import = false;

			if (validationResult.Count.Equals(0))
			{
				/* Service Validation */
				GarmentSupplierService service = (GarmentSupplierService)validationContext.GetService(typeof(GarmentSupplierService));

				if (service.DbContext.Set<GarmentSupplier>().Count(r => r._IsDeleted.Equals(false) && r.Id != this.Id && r.Code.Equals(this.Code)) > 0) /* Code Unique */
					validationResult.Add(new ValidationResult("Code already exists", new List<string> { "code" }));
			}

			return validationResult;
		}
	}
}
