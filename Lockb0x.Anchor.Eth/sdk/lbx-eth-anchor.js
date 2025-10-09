// lbx-eth-anchor.js
// Node.js SDK for Lockb0x.Anchor.Eth Ethereum Anchor

const { ethers } = require('ethers');
const ABI = [
  "event Anchored(bytes32 indexed hash, string metadata, address indexed submitter, uint256 timestamp)",
  "function anchor(bytes32 hash, string metadata) external",
  "function getAnchor(bytes32 hash) external view returns (tuple(bytes32 hash, string metadata, address submitter, uint256 timestamp))"
];

class Lockb0xEthAnchor {
  constructor(contractAddress, provider, signer) {
    this.contract = new ethers.Contract(contractAddress, ABI, signer || provider);
  }

  async anchor(hash, metadata) {
    return await this.contract.anchor(hash, metadata);
  }

  async getAnchor(hash) {
    return await this.contract.getAnchor(hash);
  }
}

module.exports = Lockb0xEthAnchor;
