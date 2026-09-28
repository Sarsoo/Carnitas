using MudBlazor;

namespace Carnitas.Web.Theme;

public class Catpuccin: MudTheme
{
    public Catpuccin()
    {
        PaletteLight = new PaletteLight()
        {
            Black = "#dce0e8", // crust
            Primary = "#8839ef", // mauve
            Info = "#1e66f5", // blue
            Success = "#40a02b", // green
            Warning = "#df8e1d", // yellow
            Error = "#d20f39", // red
            Dark = "#dce0e8", // crust
            TextPrimary = "#4c4f69", // text
            TextSecondary = "#8c8fa1", // overlay 1
            TextDisabled = "#eff1f5", // base
            ActionDefault = "#7287fd", // lavender
            ActionDisabled = "#ccd0da", // Surface 0
            // ActionDisabledBackground = "#27272f",
            Background = "#eff1f5", // base
            BackgroundGray = "#e6e9ef", //mantle
            Surface = "#ccd0da", //surface 0
            DrawerBackground = "#ccd0da", // surface 0
            DrawerText = "#4c4f69", // text
            DrawerIcon = "#7287fd", // lavender
            AppbarBackground = "#bcc0cc", // surface 1
            AppbarText = "#4c4f69", // text
            LinesDefault = "#dc8a78", // rosewater
            // LinesInputs = "",
            // TableLines = "",
            // TableStriped = "",
            Divider = "#dc8a78", // rosewater
            // DividerLight = "",
            // Skeleton = ""
        };
        
        PaletteDark = new PaletteDark()
        {
            Black = "rgb(35, 38, 52)", // crust
            Primary = "rgb(202, 158, 230)", // mauve
            Info = "rgb(140, 170, 238)", // blue
            Success = "rgb(166, 209, 137)", // green
            Warning = "rgb(229, 200, 144)", // yellow
            Error = "rgb(231, 130, 132)", // red
            Dark = "#232634", // crust
            TextPrimary = "rgb(198, 208, 245)", // text
            TextSecondary = "rgb(131, 139, 167)", // overlay 1
            TextDisabled = "rgb(48, 52, 70)", // base
            ActionDefault = "#babbf1", // lavender
            ActionDisabled = "#414559", // Surface 0
            // ActionDisabledBackground = "#27272f",
            Background = "rgb(48, 52, 70)", // base
        // {
        //     Default = new DefaultTypography
        //     {
        //         FontFamily = ["Fraunces"]
        //     }
        // };
            BackgroundGray = "rgb(41, 44, 60)", //mantle
            Surface = "rgb(65, 69, 89)", //surface 0
            DrawerBackground = "rgb(65, 69, 89)", // surface 0
            DrawerText = "rgb(198, 208, 245)", // text
            DrawerIcon = "rgb(186, 187, 241)", // lavender
            AppbarBackground = "#51576d", // surface 1
            AppbarText = "#c6d0f5", // text
            LinesDefault = "#f2d5cf", // rosewater
            // LinesInputs = "",
            // TableLines = "",
            // TableStriped = "",
            Divider = "rgb(242, 213, 207)", // rosewater
            // DividerLight = "",
            // Skeleton = ""
        };

        // Typography = new Typography
    }
}