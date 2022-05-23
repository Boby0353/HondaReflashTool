﻿using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Forms;

public class Class_Checksums
{
    private GForm_Main GForm_Main_0;

    public void Load(ref GForm_Main GForm_Main_1)
    {
        GForm_Main_0 = GForm_Main_1;
    }

    public byte GetNegativeChecksumArea(byte[] byte_1, int Start, int ChecksumLocation)
    {
        byte b = 0;
        for (int i = Start; i < byte_1.Length; i++)
        {
            if (i != ChecksumLocation)
            {
                b -= byte_1[i];
            }
        }
        return b;
    }

    public int GetChecksumLocationThisECU(string ThisECU)
    {
        int CheckLocation = 0;
        if (GForm_Main_0.Editortable_0.LoadDefinitionsFor(ThisECU))
        {
            if (GForm_Main_0.Editortable_0.ClassEditor_0.DefinitionsChecksumLocation != "")
            {
                CheckLocation = int.Parse(GForm_Main_0.Editortable_0.ClassEditor_0.DefinitionsChecksumLocation.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            }
        }
        return CheckLocation;
    }

    public int GetChecksumLocation(byte[] BinFileBytes)
    {
        byte[] BufferBytes = BinFileBytes;
        int CheckLocation = 0;

        string Thisecuu = GForm_Main_0.Editortable_0.ExtractECUNameFromThisFile(BinFileBytes);
        if (Thisecuu != "")
        {
            if (GForm_Main_0.Editortable_0.LoadDefinitionsFor(Thisecuu))
            {
                if (GForm_Main_0.Editortable_0.ClassEditor_0.DefinitionsChecksumLocation != "")
                {
                    CheckLocation = int.Parse(GForm_Main_0.Editortable_0.ClassEditor_0.DefinitionsChecksumLocation.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                }
            }
        }

        if (CheckLocation == 0)
        {
            //HERE
            //GForm_Main_0.method_1("Checksum location not definied for '" + Thisecuu + "', using known checksum location but can possibly be wrong on some ecu!");
            
            DialogResult result = DarkMessageBox.Show("Checksum location not definied for '" + Thisecuu + "'" + Environment.NewLine + "Do you want to use 'known good' checksum location but they still can possibly be wrong on some ecu?", "Checksum location", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.Yes)
            {
                if (BufferBytes.Length - 1 == 0xF7FFF) CheckLocation = 0x400;       //1mb-fw  -> 0x8400 in full bin but we dont have the bootloader 0x0000 to 0x8000
                if (BufferBytes.Length - 1 == 0xFFFFF) CheckLocation = 0x8400;      //1mb-full
                if (BufferBytes.Length - 1 == 0x1EFFFF) CheckLocation = 0x12;       //2mb-fw
                if (BufferBytes.Length - 1 == 0x1FFFFF) CheckLocation = 0x10012;    //2mb-full
                if (BufferBytes.Length - 1 == 0x26FFFF) CheckLocation = 0x1F03E6;   //4mb-fw
                if (BufferBytes.Length - 1 == 0x27FFFF) CheckLocation = 0x2003E6;   //4mb-full       //0x3FFFFF
            }
        }

        return CheckLocation;
    }

    public byte[] VerifyChecksumFullBin(byte[] BinFileBytes)
    {
        //###############################
        //Get Checksum and Fix it
        byte[] BufferBytes = BinFileBytes;
        int CheckLocation = GetChecksumLocation(BinFileBytes);

        if (CheckLocation == 0)
        {
            GForm_Main_0.method_1("Checksum location not found!");
            return BufferBytes;
        }

        byte num = BufferBytes[CheckLocation];
        byte num2 = GetNegativeChecksumArea(BufferBytes, 0, CheckLocation);
        if (num != num2)
        {
            GForm_Main_0.method_1("Checksum miss match.");
            BufferBytes[CheckLocation] = num2;
            GForm_Main_0.method_1("Checksum fixed at 0x" + CheckLocation.ToString("X") + " | Checksum: 0x" + num2.ToString("X2"));
        }
        else
        {
            GForm_Main_0.method_1("Checksum are good at 0x" + CheckLocation.ToString("X") + " | Checksum: 0x" + num2.ToString("X2"));
        }
        return BufferBytes;
    }

    public byte[] VerifyChecksumFWBin(byte[] FWFileBytes)
    {
        //###############################
        //Get Checksum and Fix it
        byte[] BufferBytes = FWFileBytes;
        int CheckLocation = GetChecksumLocation(FWFileBytes);

        if (CheckLocation == 0)
        {
            GForm_Main_0.method_1("Checksum location not found!");
            return BufferBytes;
        }

        byte num = Class_RWD.BootloaderSum;
        byte num2 = Class_RWD.GetNegativeChecksumFWBin(BufferBytes, CheckLocation);
        byte ThisSum = num;
        ThisSum -= num2;
        byte chk = BufferBytes[CheckLocation];
        if (chk != ThisSum)
        {
            GForm_Main_0.method_1("Checksum miss match.");
            BufferBytes[CheckLocation] = ThisSum;
            GForm_Main_0.method_1("Checksum fixed at 0x" + CheckLocation.ToString("X") + " | Checksum: 0x" + num2.ToString("X2"));
        }
        else
        {
            GForm_Main_0.method_1("checksum good at 0x" + CheckLocation.ToString("X") + " | Checksum: 0x" + num2.ToString("X2"));
        }
        return BufferBytes;
    }
}
