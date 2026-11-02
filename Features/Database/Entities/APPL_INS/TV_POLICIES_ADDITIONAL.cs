using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("TV_POLICIES_ADDITIONAL")]
public class TV_POLICIES_ADDITIONAL
{
    [Column("ADD_POL_NO")]
    public required string ADD_POL_NO { get; set; }

    [Column("ADD_POL_YEAR")]
    public int ADD_POL_YEAR { get; set; }

    [Column("ADD_POL_ENDT_NO")]
    public required string ADD_POL_ENDT_NO { get; set; }

    [Column("ADD_POL_ENDT_YEAR")]
    public int ADD_POL_ENDT_YEAR { get; set; }

    [Column("ADD_POL_CLS_ID")]
    public required string ADD_POL_CLS_ID { get; set; }

    [Column("ADD_POL_PLT_ID")]
    public required string ADD_POL_PLT_ID { get; set; }

    [Column("ADD_POL_CMP_ID")]
    public int ADD_POL_CMP_ID { get; set; }

    [Column("ADD_POL_CGP_ID")]
    public int ADD_POL_CGP_ID { get; set; }

    [Column("ADD_POL_DCT_ID")]
    public required string ADD_POL_DCT_ID { get; set; }

    [Column("ADD_POL_OFC_ID")]
    public required string ADD_POL_OFC_ID { get; set; }

    [Column("ADD_POL_CBR_ID")]
    public required string ADD_POL_CBR_ID { get; set; }

    [Column("ADD_POL_INT_SERIAL")]
    public int ADD_POL_INT_SERIAL { get; set; }

    [Column("ADD_F01")]
    public string? ADD_F01 { get; set; }
    public bool CovidFlag => ADD_F01 == "1";

    [Column("ADD_F02")]
    public string? ADD_F02 { get; set; }
}
