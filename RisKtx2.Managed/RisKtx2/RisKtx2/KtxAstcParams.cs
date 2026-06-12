using System.Runtime.InteropServices;

namespace RisKtx2;


/// <summary>
/// The parameters for the ASTC compression algorithm.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class KtxAstcParams
{
    /// <summary>
    /// A swizzle to apply before encoding.
    /// It must match the regular expression /^[rgba01]{4}$/.
    /// </summary>
    public char[] InputSwizzle { get; set; }
    
    /// <summary>
    /// TODO:
    /// </summary>
    public bool Verbose { get; set; }
    
    /// <summary>
    /// The quality level of the ASTC compression algorithm.
    /// </summary>
    public KtxPackAstcQualityLevels QualityLevel { get; set; }
    
    /// <summary>
    /// Converts this managed KtxBasisParams instance to its native representation (ris_ktxBasisParams) for use in interop calls.
    /// </summary>
    /// <returns>The <see cref="ris_ktxBasisParams"/>.</returns>
    internal ris_ktxAstcParams ToNative()
    {
        ris_ktxAstcParams risKtxBasisParams = new()
        {
            qualityLevel = QualityLevel,
            verbose = Verbose ? 1u : 0,
        };

        // Handle the InputSwizzle array. We need to pin it in memory to get a stable pointer for the native code.
        if (InputSwizzle != null)
        {
            if (InputSwizzle.Length != 4)
            {
                throw new ArgumentException("InputSwizzle must be an array of 4 characters.", nameof(InputSwizzle));
            }

            risKtxBasisParams.inputSwizzleR = (byte) InputSwizzle[0];
            risKtxBasisParams.inputSwizzleG = (byte) InputSwizzle[1];
            risKtxBasisParams.inputSwizzleB = (byte) InputSwizzle[2];
            risKtxBasisParams.inputSwizzleA = (byte) InputSwizzle[3];
        }

        return risKtxBasisParams;
    }
}

[StructLayout( LayoutKind.Sequential)]
internal struct ris_ktxAstcParams {
    
    public KtxPackAstcQualityLevels qualityLevel;
    public uint verbose;
    public byte inputSwizzleR;
    public byte inputSwizzleG;
    public byte inputSwizzleB;
    public byte inputSwizzleA;
}