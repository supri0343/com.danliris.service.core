﻿using Com.DanLiris.Service.Core.Lib.Models;
using Com.Moonlay.NetCore.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Com.DanLiris.Service.Core.Lib.Helpers;
using Newtonsoft.Json;
using System.Reflection;
using Com.Moonlay.NetCore.Lib;
using Com.DanLiris.Service.Core.Lib.ViewModels;
using CsvHelper.Configuration;
using System.Dynamic;
using Com.DanLiris.Service.Core.Lib.Interfaces;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Com.Moonlay.Models;
using Com.DanLiris.Service.Core.Lib.Helpers.IdentityService;
using System.IO;
using System.Data;
using System.Globalization;

namespace Com.DanLiris.Service.Core.Lib.Services
{
	public class GarmentSupplierService : BasicService<CoreDbContext, GarmentSupplier>, IBasicUploadCsvService<GarmentSupplierViewModel>, IMap<GarmentSupplier, GarmentSupplierViewModel>
	{
		private const string UserAgent = "core-product-service";
		protected IIdentityService _IdentityService;
		protected DbSet<Supplier> _DbSet;
		private readonly CoreDbContext _dbContext;
		private readonly string[] ImportAllowed = { "True", "False" };
		private readonly string[] UseVatAllowed = { "True", "False" };
		private readonly string[] UseTaxAllowed = { "True", "False" };
		public GarmentSupplierService(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
		public override Tuple<List<GarmentSupplier>, int, Dictionary<string, string>, List<string>> ReadModel(int Page = 1, int Size = 25, string Order = "{}", List<string> Select = null, string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentSupplier> Query = this.DbContext.GarmentSuppliers;
			Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Filter);
			Query = ConfigureFilter(Query, FilterDictionary);
			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			/* Search With Keyword */
			if (Keyword != null)
			{
				List<string> SearchAttributes = new List<string>()
				{
					"Code", "Name","Address"
				};

				Query = Query.Where(General.BuildSearch(SearchAttributes), Keyword);
			}

			/* Const Select */
			List<string> SelectedFields = new List<string>()
			{
				"Id", "code", "name", "address", "country", "import", "NPWP", "usevat", "usetax", "IncomeTaxes", "IsPosted"
			};

			Query = Query
				.Select(s => new GarmentSupplier
				{
					Id = s.Id,
					Code = s.Code,
					Name = s.Name,
					Address = s.Address,
					Country = s.Country,
					Import = s.Import,
					NPWP = s.NPWP,
					UseVat = s.UseVat,
					UseTax = s.UseTax,
					IncomeTaxesId = s.IncomeTaxesId,
					IncomeTaxesName = s.IncomeTaxesName,
					IncomeTaxesRate = s.IncomeTaxesRate,
					_LastModifiedUtc =s._LastModifiedUtc,
					Active = s.Active
				}).OrderByDescending(b => b._LastModifiedUtc);

			/* Order */
			if (OrderDictionary.Count.Equals(0))
			{
				OrderDictionary.Add("_updatedDate", General.DESCENDING);

				Query = Query.OrderByDescending(b => b._LastModifiedUtc); /* Default Order */
			}
			else
			{
				string Key = OrderDictionary.Keys.First();
				string OrderType = OrderDictionary[Key];
				string TransformKey = General.TransformOrderBy(Key);

				BindingFlags IgnoreCase = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

				Query = OrderType.Equals(General.ASCENDING ) ?
					Query.OrderBy(b => b.GetType().GetProperty(TransformKey, IgnoreCase).GetValue(b)) :
					Query.OrderByDescending(b => b.GetType().GetProperty(TransformKey, IgnoreCase).GetValue(b));
			}

			/* Pagination */
			Pageable<GarmentSupplier> pageable = new Pageable<GarmentSupplier>(Query , Page - 1, Size);
			List<GarmentSupplier> Data = pageable.Data.ToList<GarmentSupplier>();

			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary, SelectedFields);
		}
		public GarmentSupplierViewModel MapToViewModel(GarmentSupplier GarmentSupplier)
		{
			GarmentSupplierViewModel GarmentSupplierVM = new GarmentSupplierViewModel();

			GarmentSupplierVM.Id = GarmentSupplier.Id;
			GarmentSupplierVM.UId = GarmentSupplier.UId;
			GarmentSupplierVM._IsDeleted = GarmentSupplier._IsDeleted;
			GarmentSupplierVM.Active = GarmentSupplier.Active;
			GarmentSupplierVM._CreatedUtc = GarmentSupplier._CreatedUtc;
			GarmentSupplierVM._CreatedBy = GarmentSupplier._CreatedBy;
			GarmentSupplierVM._CreatedAgent = GarmentSupplier._CreatedAgent;
			GarmentSupplierVM._LastModifiedUtc = GarmentSupplier._LastModifiedUtc;
			GarmentSupplierVM._LastModifiedBy = GarmentSupplier._LastModifiedBy;
			GarmentSupplierVM._LastModifiedAgent = GarmentSupplier._LastModifiedAgent;
			GarmentSupplierVM.code = GarmentSupplier.Code;
			GarmentSupplierVM.name = GarmentSupplier.Name;
			GarmentSupplierVM.address = GarmentSupplier.Address;
			GarmentSupplierVM.country = GarmentSupplier.Country;
			GarmentSupplierVM.contact = GarmentSupplier.Contact;
			GarmentSupplierVM.PIC = GarmentSupplier.PIC;
			GarmentSupplierVM.import = GarmentSupplier.Import;
			GarmentSupplierVM.usevat = GarmentSupplier.UseVat;
			GarmentSupplierVM.usetax = GarmentSupplier.UseTax;
			GarmentSupplierVM.IncomeTaxes = new IncomeTaxViewModel
			{
				Id = GarmentSupplier.IncomeTaxesId,
				name = GarmentSupplier.IncomeTaxesName,
				rate = GarmentSupplier.IncomeTaxesRate,
				Rate = GarmentSupplier.IncomeTaxesRate
			};
			GarmentSupplierVM.NPWP = GarmentSupplier.NPWP;
			GarmentSupplierVM.serialNumber = GarmentSupplier.SerialNumber;
			GarmentSupplierVM.description = GarmentSupplier.Description;
			GarmentSupplierVM.IsPosted = GarmentSupplier.Active;
			

			return GarmentSupplierVM;
		}
		public GarmentSupplier MapToModel(GarmentSupplierViewModel GarmentSupplierVM)
		{
			GarmentSupplier GarmentSupplier = new GarmentSupplier();

			GarmentSupplier.Id = GarmentSupplierVM.Id;
			GarmentSupplier.UId = GarmentSupplierVM.UId;
			GarmentSupplier._IsDeleted = GarmentSupplierVM._IsDeleted;
			GarmentSupplier.Active = GarmentSupplierVM.Active;
			GarmentSupplier._CreatedUtc = GarmentSupplierVM._CreatedUtc;
			GarmentSupplier._CreatedBy = GarmentSupplierVM._CreatedBy;
			GarmentSupplier._CreatedAgent = GarmentSupplierVM._CreatedAgent;
			GarmentSupplier._LastModifiedUtc = GarmentSupplierVM._LastModifiedUtc;
			GarmentSupplier._LastModifiedBy = GarmentSupplierVM._LastModifiedBy;
			GarmentSupplier._LastModifiedAgent = GarmentSupplierVM._LastModifiedAgent;
			GarmentSupplier.Code = GarmentSupplierVM.code;
			GarmentSupplier.Name = GarmentSupplierVM.name;
			GarmentSupplier.Address = GarmentSupplierVM.address;
			GarmentSupplier.Country = GarmentSupplierVM.country;
			GarmentSupplier.Contact = GarmentSupplierVM.contact;
			GarmentSupplier.PIC = GarmentSupplierVM.PIC;
			GarmentSupplier.Import = !Equals(GarmentSupplierVM.import, null) ? Convert.ToBoolean(GarmentSupplierVM.import) : false;
			GarmentSupplier.UseVat = !Equals(GarmentSupplierVM.usevat, null) ? Convert.ToBoolean(GarmentSupplierVM.usevat) : false;
			GarmentSupplier.UseTax = !Equals(GarmentSupplierVM.usetax, null) ? Convert.ToBoolean(GarmentSupplierVM.usetax) : false; /* Check Null */
			if (GarmentSupplierVM.IncomeTaxes != null)
			{
				GarmentSupplier.IncomeTaxesId = GarmentSupplierVM.IncomeTaxes.Id;
				GarmentSupplier.IncomeTaxesName = GarmentSupplierVM.IncomeTaxes.name;
				GarmentSupplier.IncomeTaxesRate = !Equals(GarmentSupplierVM.IncomeTaxes.rate, null) ? Convert.ToDouble(GarmentSupplierVM.IncomeTaxes.rate) : null;
            }
			else
			{
				GarmentSupplier.IncomeTaxesId = 1;
				GarmentSupplier.IncomeTaxesName = "";
				GarmentSupplier.IncomeTaxesRate = 0;
			}
			GarmentSupplier.NPWP = GarmentSupplierVM.NPWP;
			GarmentSupplier.SerialNumber = GarmentSupplierVM.serialNumber;
			GarmentSupplier.Description = GarmentSupplierVM.description;
			GarmentSupplier.Active = GarmentSupplierVM.IsPosted;
			
			return GarmentSupplier;
		}

		public async Task<int> GarmentSupplierPost(List<GarmentSupplierViewModel> garmentsupplier, string username)
		{
			int Updated = 1;
			var Ids = garmentsupplier.Select(d => d.Id).ToList();
			var listData = this.DbContext.GarmentSuppliers.Where(m => Ids.Contains(m.Id) && !m._IsDeleted).ToList();

			listData.ForEach(async m =>
			{
				Updated = await garmentsupplierUpdated(m, username);

			});

			return Updated;
		}

		public async Task<int> garmentsupplierUpdated(GarmentSupplier model, string username)
		{


			model.Active = true;
			model.FlagForUpdate("username", UserAgent);


			DbContext.GarmentSuppliers.Update(model);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> garmentsupplierNonActive(int Id, string username)
		{

			var model = this.DbContext.GarmentSuppliers.FirstOrDefault(x => x.Id == Id);

			model.Active = false;
			model.FlagForUpdate(username, UserAgent);

			DbContext.GarmentSuppliers.Update(model);
			return await DbContext.SaveChangesAsync();
		}

		/* Upload CSV */
		private readonly List<string> Header = new List<string>()
		{
			"Kode","Nama Supplier","Alamat","Negara","Kontak","PIC","Import","Kena PPN","Kena PPH","Jenis PPH","Rate PPH","NPWP","Serial Number","Description"
		};
		public List<string> CsvHeader => Header;

		public sealed class GarmentSupplierMap : ClassMap<GarmentSupplierViewModel>
		{
			public GarmentSupplierMap()
			{

				Map(s => s.code).Index(0);
				Map(s => s.name).Index(1);
				Map(s => s.address).Index(2);
				Map(s => s.country).Index(3);
				Map(s => s.contact).Index(4);
				Map(s => s.PIC).Index(5);
				Map(s => s.import).Index(6).TypeConverter<StringConverter>();
				Map(s => s.usevat ).Index(7).TypeConverter<StringConverter>();
				Map(s => s.usetax).Index(8).TypeConverter<StringConverter>();
				Map(s => s.IncomeTaxes.name).Index(9);
				Map(s => s.IncomeTaxes.rate).Index(10).TypeConverter<StringConverter>();
				Map(s => s.NPWP).Index(11);
				Map(s => s.serialNumber).Index(12);
				Map(s => s.description).Index(13);

			}
		}

		public Tuple<bool, List<object>> UploadValidate(List<GarmentSupplierViewModel> Data, List<KeyValuePair<string, StringValues>> Body)
		{
			List<object> ErrorList = new List<object>();
			string ErrorMessage;
			bool Valid = true;
			IncomeTax incomeTax = null;

			foreach (GarmentSupplierViewModel GarmentSupplierVM in Data)
			{
				ErrorMessage = "";

				if (string.IsNullOrWhiteSpace(GarmentSupplierVM.code))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kode tidak boleh kosong, ");
				}
				else if (Data.Any(d => d != GarmentSupplierVM && d.code.Equals(GarmentSupplierVM.code)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kode tidak boleh duplikat, ");
				}

				if (string.IsNullOrWhiteSpace(GarmentSupplierVM.name))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Nama tidak boleh kosong, ");
				}

				if (string.IsNullOrWhiteSpace(Convert.ToString(GarmentSupplierVM.import)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Import tidak boleh kosong, ");
				}
				else if (!ImportAllowed.Any(i => i.Equals(Convert.ToString(GarmentSupplierVM.import), StringComparison.CurrentCultureIgnoreCase)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Import harus diisi dengan True atau False, ");
				}
				if (string.IsNullOrWhiteSpace(Convert.ToString(GarmentSupplierVM.usevat)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kena PPN tidak boleh kosong, ");
				}
				else if (!UseVatAllowed.Any(i => i.Equals(Convert.ToString(GarmentSupplierVM.usevat), StringComparison.CurrentCultureIgnoreCase)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kena PPN harus diisi dengan True atau False, ");
				}
				if (string.IsNullOrWhiteSpace(Convert.ToString(GarmentSupplierVM.usetax)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kena PPH tidak boleh kosong, ");
				}
				else if (!UseTaxAllowed.Any(i => i.Equals(Convert.ToString(GarmentSupplierVM.usetax), StringComparison.CurrentCultureIgnoreCase)))
				{
					ErrorMessage = string.Concat(ErrorMessage, "Kena PPH harus diisi dengan True atau False, ");
                }
                bool tax;
                bool.TryParse(Convert.ToString(GarmentSupplierVM.usetax), out tax);
                double Rate = 0;
                var isIncometaxRateNumber = double.TryParse(Convert.ToString(GarmentSupplierVM.IncomeTaxes.rate), out Rate);
                if (tax == true)
                {
                    if (string.IsNullOrWhiteSpace(GarmentSupplierVM.IncomeTaxes.name))
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Jenis PPH tidak boleh kosong, ");
                    }
                    string[] RateSplit = Convert.ToString(GarmentSupplierVM.IncomeTaxes.rate).Split('.');
                    
                    if (string.IsNullOrWhiteSpace(Convert.ToString(GarmentSupplierVM.IncomeTaxes.rate)))
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Rate PPH tidak boleh kosong, ");
                    }
                    else if (!isIncometaxRateNumber)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Rate PPH harus numerik, ");
                    }
                    else if (Rate < 0 || Rate == 0)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Rate PPH harus lebih besar dari 0, ");
                    }
                    else if (RateSplit.Count().Equals(2) && RateSplit[1].Length > 2)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Kurs maksimal memiliki 2 digit dibelakang koma, ");
                    }
                    IncomeTax suppliers = DbContext.IncomeTaxes.FirstOrDefault(s => s.Name == GarmentSupplierVM.IncomeTaxes.name && s.Rate == Rate);
                    if (suppliers == null)
                    {
                        IncomeTax incometaxesname = DbContext.IncomeTaxes.FirstOrDefault(s => s.Name == GarmentSupplierVM.IncomeTaxes.name);
                        if (incometaxesname == null && GarmentSupplierVM.IncomeTaxes.name != "")
                        {
                            ErrorMessage = string.Concat(ErrorMessage, "Jenis PPH Tidak Ada di Master PPH, ");
                        }
                        IncomeTax incometaxesrate = DbContext.IncomeTaxes.FirstOrDefault(s => s.Rate == Rate);
                        if (incometaxesrate == null && Rate != 0)
                        {
                            ErrorMessage = string.Concat(ErrorMessage, "Rate PPH Tidak Ada di Master PPH, ");
                        }
                        if (incometaxesrate != null && incometaxesname != null)
                        {
                            ErrorMessage = string.Concat(ErrorMessage, " Jenis PPH dan Rate PPH tidak ada di Master PPH, ");
                        }

                    }
                    else
                    {
                        GarmentSupplierVM.IncomeTaxes.Id = suppliers.Id;
                        GarmentSupplierVM.IncomeTaxes.name = suppliers.Name;
                        GarmentSupplierVM.IncomeTaxes.rate = suppliers.Rate;
                    }
                }
                else if (tax == false)
                {
                    if (GarmentSupplierVM.IncomeTaxes.name != "" && Rate != 0)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, " Jenis PPH / Rate PPH harus kosong, ");
                    }
                    else if(GarmentSupplierVM.IncomeTaxes.name != "")
                    {
                        ErrorMessage = string.Concat(ErrorMessage, " Jenis PPH harus kosong, ");
                    }
                    else if (Rate != 0)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, " Rate PPH harus kosong, ");
                    }
                    else
                    {
                        GarmentSupplierVM.IncomeTaxes.Id = 1;
                        GarmentSupplierVM.IncomeTaxes.name = "";
                        GarmentSupplierVM.IncomeTaxes.rate = 0;
                    }
                    
                }

                
                if (string.IsNullOrEmpty(ErrorMessage))
				{
					/* Service Validation */
					incomeTax = this.DbContext.Set<IncomeTax>().FirstOrDefault(d => d._IsDeleted.Equals(false) );
					if (this.DbSet.Any(d => d._IsDeleted.Equals(false) && d.Code.Equals(GarmentSupplierVM.code)))
					{
						ErrorMessage = string.Concat(ErrorMessage, "Kode tidak boleh duplikat, ");
					}
					if (incomeTax==null)
					{
						ErrorMessage = string.Concat(ErrorMessage, "PPH tidak terdaftar dalam master Income Tax");
					}
				}

				if (string.IsNullOrEmpty(ErrorMessage))
				{
					GarmentSupplierVM.import = Convert.ToBoolean(GarmentSupplierVM.import);
				}
				else
				{
					ErrorMessage = ErrorMessage.Remove(ErrorMessage.Length - 2);
					var Error = new ExpandoObject() as IDictionary<string, object>;

					Error.Add("Kode", GarmentSupplierVM.code);
					Error.Add("Nama Supplier", GarmentSupplierVM.name);
					Error.Add("Alamat", GarmentSupplierVM.address);
					Error.Add("Negara", GarmentSupplierVM.country);
					Error.Add("Kontak", GarmentSupplierVM.code);
					Error.Add("PIC", GarmentSupplierVM.PIC);
					Error.Add("Import", GarmentSupplierVM.import);
					Error.Add("Kena PPN", GarmentSupplierVM.usevat);
					Error.Add("Kena PPH", GarmentSupplierVM.usetax);
					Error.Add("Jenis PPH", GarmentSupplierVM.IncomeTaxes.name);
					Error.Add("Rate PPH", GarmentSupplierVM.IncomeTaxes.rate);
					Error.Add("NPWP", GarmentSupplierVM.NPWP);
					Error.Add("Serial Number", GarmentSupplierVM.serialNumber);
					Error.Add("Description", GarmentSupplierVM.description);
					Error.Add("Error", ErrorMessage);

					ErrorList.Add(Error);
				}
			}

			if (ErrorList.Count > 0)
			{
				Valid = false;
			}

			return Tuple.Create(Valid, ErrorList);
		}

        public List<GarmentSupplier> GetByCodes(string code)
        {
            var codes = code.Split(",");
            //return this.DbSet.IgnoreQueryFilters().FirstOrDefault(p => code == p.Code);
            return this.DbSet.IgnoreQueryFilters().Where(x => codes.Contains(x.Code)).Select(x => x).ToList();
        }

        //
        public IQueryable<XLSGarmentSupplierViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
           
			var NewQuery = (from a in this.DbContext.GarmentSuppliers 
                            where a._IsDeleted == false 
						       && a._CreatedUtc.AddHours(7).Date >= DateFrom.Date &&
                               a._CreatedUtc.AddHours(7).Date <= DateTo.Date

                         select new XLSGarmentSupplierViewModel
						 {
							 createddate = a._CreatedUtc,
							 code = a.Code,
							 name = a.Name,
							 address = a.Address,
							 country = a.Country,
							 import = a.Import == true ? "IMPORT" : "LOKAL",
							 NPWP = a.NPWP,
							 contact = a.Contact,
							 PIC = a.PIC,
							 usevat = a.UseVat == true ? "YA" : "TIDAK",
							 usetax = a.UseTax == true ? "YA" : "TIDAK",
							 taxname = a.IncomeTaxesName,
							 taxrate = a.IncomeTaxesRate,
							 serialNumber = a.SerialNumber,
							 description = a.Description,
							 Aktif = a.Active == true ? "SUDAH" : "BELUM"
	                     });
            return NewQuery;
        }

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo)
        {
            var Query = GetReportQuery(dateFrom, dateTo);
            Query = Query.OrderBy(b => b.code);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TGL INPUT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ALAMAT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NEGARA", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "IMPORT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NPWP", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KONTAK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PIC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KENA PPN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "KENA PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RATE PPH", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "SERIAL NUMBER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "AKTIF", DataType = typeof(String) });


            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                var index = 0;
                foreach (var item in Query)
                {
					index++;
                    string CreatedDate = item.createddate == new DateTime(1970, 1, 1) ? "-" : item.createddate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, CreatedDate, item.code, item.name, item.address, item.country, item.import, item.NPWP, item.contact, item.PIC, item.usevat, item.usetax, item.taxname, item.taxrate, item.serialNumber, item.description, item.Aktif);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Sheet1") }, true);

        }
    }
}
