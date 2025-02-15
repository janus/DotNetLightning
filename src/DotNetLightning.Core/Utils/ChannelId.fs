namespace DotNetLightning.Utils

open NBitcoin

type ChannelId =
    | ChannelId of uint256

    member this.Value = let (ChannelId v) = this in v

    static member Zero = uint256.Zero |> ChannelId
