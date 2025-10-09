// Example CLI for Lockb0x Ethereum Anchor
import { ethers } from "ethers";
import Lockb0xEthAnchor from "../lbx-eth-anchor.js";
import fs from "fs";

const CONTRACT_ADDRESS = process.env.CONTRACT_ADDRESS;
const PROVIDER_URL = process.env.PROVIDER_URL;
const PRIVATE_KEY = process.env.PRIVATE_KEY;

const provider = new ethers.JsonRpcProvider(PROVIDER_URL);
const signer = new ethers.Wallet(PRIVATE_KEY, provider);
const anchor = new Lockb0xEthAnchor(CONTRACT_ADDRESS, provider, signer);

async function main() {
  const cmd = process.argv[2];
  if (cmd === "anchor") {
    const filePath = process.argv[3];
    const metadata = process.argv[4] || "";
    const file = fs.readFileSync(filePath);
    const hash = ethers.keccak256(file);
    const tx = await anchor.anchor(hash, metadata);
    console.log("Anchored:", tx.hash);
  } else if (cmd === "get") {
    const hash = process.argv[3];
    const anchorData = await anchor.getAnchor(hash);
    console.log(anchorData);
  } else {
    console.log("Usage: cli.mjs anchor <file> [metadata] | get <hash>");
  }
}

main();
