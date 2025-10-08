// scripts/deploy.js
const hre = require("hardhat");

async function main() {
  const Lockb0xAnchor = await hre.ethers.getContractFactory("Lockb0x_Anchor_Eth");
  const contract = await Lockb0xAnchor.deploy();
  await contract.deployed();
  console.log("Lockb0xAnchor deployed to:", contract.address);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
