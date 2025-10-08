// scripts/verify.js
const hre = require("hardhat");

async function main() {
  const contractAddress = process.argv[2];
  if (!contractAddress) {
    console.error("Usage: node scripts/verify.js <contractAddress>");
    process.exit(1);
  }
  await hre.run("verify:verify", {
    address: contractAddress,
    constructorArguments: [],
  });
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
