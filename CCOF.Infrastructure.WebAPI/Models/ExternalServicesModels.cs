using System.ComponentModel.DataAnnotations;

namespace CCOF.Infrastructure.WebAPI.Models;

public record InvoiceHeader
{
    public required string feederNumber { get; set; }
    public required string headertransactionType { get; set; }
    public required string batchType { get; set; }
    public required string delimiter { get; set; }
    [StringLength(9)]
    public required string supplierNumber { get; set; }
    [StringLength(3)]
    public required string supplierSiteNumber { get; set; }
    [StringLength(50)]
    public required string invoiceNumber { get; set; }
    [StringLength(2)]
    public required string invoiceType { get; set; }
    [StringLength(8)]
    public required string invoiceDate { get; set; }
    [StringLength(8)]
    public required string invoiceRecDate { get; set; }
    [StringLength(8)]
    public required string goodsDate { get; set; }
    [StringLength(20)]
    public required string PONumber { get; set; }
    [StringLength(9)]
    public required string payGroupLookup { get; set; }
    [StringLength(4)]
    public required string remittanceCode { get; set; }
    [StringLength(15)]
    public required string grossInvoiceAmount { get; set; }
    [StringLength(3)]
    public required string CAD { get; set; }
    [StringLength(50)]
    public required string termsName { get; set; }
    [StringLength(60)]
    public required string description { get; set; }
    [StringLength(30)]
    public required string oracleBatchName { get; set; }
    public required string payflag { get; set; }
    [StringLength(110)]
    public required string flow { get; set; }
    [StringLength(9)]
    public required string SIN { get; set; }
    public List<InvoiceLines>? invoiceLines { get; set; }
}

public record InvoiceLines
{
    public required string feederNumber { get; set; }
    public required string batchType { get; set; }
    public required string delimiter { get; set; }
    public required string linetransactionType { get; set; }
    [StringLength(50)]
    public required string invoiceNumber { get; set; }
    [StringLength(4)]
    public required string invoiceLineNumber { get; set; }
    [StringLength(9)]
    public required string supplierNumber { get; set; }
    [StringLength(3)]
    public required string supplierSiteNumber { get; set; }
    [StringLength(4)]
    public required string committmentLine { get; set; }
    [StringLength(15)]
    public required string lineAmount { get; set; }
    [StringLength(1)]
    public required string lineCode { get; set; }
    [StringLength(50)]
    public required string distributionACK { get; set; }
    [StringLength(55)]
    public required string lineDescription { get; set; }
    [StringLength(8)]
    public required string effectiveDate { get; set; }
    [StringLength(10)]
    public required string quantity { get; set; }
    [StringLength(15)]
    public required string unitPrice { get; set; }
    [StringLength(163)]
    public required string optionalData { get; set; }
    [StringLength(30)]
    public required string distributionSupplierNumber { get; set; }
    [StringLength(110)]
    public required string flow { get; set; }

}