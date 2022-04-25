namespace DotNetLightning.Utils

open System
open System.IO
open NBitcoin
open NBitcoin.Crypto

[<AutoOpen>]
module NBitcoinExtensions =
    module FeeRate =
        let FeePerKWeight(feeRate: FeeRate) =
            Money.Satoshis(feeRate.FeePerK.Satoshi / 4L)

    type NBitcoin.Transaction with

        member this.GetTxId() =
            TxId(this.GetHash())

    type Money with

        member this.ToLNMoney() =
            LNMoney.Satoshis(this.Satoshi)

    type OutPoint with

        member this.ToChannelId() : ChannelId =
            let mutable res = this.Clone().Hash.ToBytes()
            res.[30] <- res.[30] ^^^ (uint8(this.N >>> 8) &&& 0xffuy)
            res.[31] <- res.[31] ^^^ (uint8(this.N >>> 0) &&& 0xffuy)
            res |> uint256 |> ChannelId

    type TxOut with

        static member LexicographicCompare
            (txOut0: TxOut)
            (txOut1: TxOut)
            : int =
            if txOut0.Value < txOut1.Value then
                -1
            elif txOut0.Value > txOut1.Value then
                1
            else
                let script0 = txOut0.ScriptPubKey.ToBytes()
                let script1 = txOut1.ScriptPubKey.ToBytes()

                let rec compare(index: int) =
                    if script0.Length = index && script1.Length = index then
                        0
                    elif script0.Length = index then
                        -1
                    elif script1.Length = index then
                        1
                    elif script0.[index] < script1.[index] then
                        -1
                    elif script0.[index] > script1.[index] then
                        1
                    else
                        compare(index + 1)

                compare 0

    type PSBT with

        member this.GetMatchingSig(pubkey: PubKey) =
            this.Inputs
            |> Seq.collect(fun i -> i.PartialSigs)
            |> Seq.choose(fun kv ->
                if kv.Key = pubkey then
                    Some kv.Value
                else
                    None
            )
            |> Seq.tryExactlyOne

        member this.GetTxId() =
            this.GetGlobalTransaction().GetTxId()

    type Key with

        static member BytesLength: int = 32

        static member FromHashOf(preimage: array<byte>) : Key =
            new Key(Hashes.SHA256 preimage)

    type PubKey with

        static member BytesLength: int = 33

    type DateTimeOffset with

        member this.IsValidUnixTime() : bool =
            let unixRef = DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)
            let dt = this.ToUniversalTime()

            (unixRef <= dt)
            && ((dt - unixRef).TotalSeconds <= float UInt32.MaxValue)
