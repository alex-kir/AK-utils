# AK-utils
AK-utils is a set of lightweight utils, helpers and wrapers.
Each file has minimum dependices from other files of library.

$ DatabaseContext

db.Create("IMAGE", new[] { "image_pk" }, new[] { "bytes", "ratio" });

using (var result = db.Execute("SELECT * FROM `IMAGE` WHERE `image_pk` = '" + image + "';"))
{
  var row = result.FirstOrDefault();
  if (row != null)
    return row["bytes"] as byte[];
}
