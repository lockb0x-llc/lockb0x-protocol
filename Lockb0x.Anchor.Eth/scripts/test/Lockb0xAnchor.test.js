const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("Lockb0xAnchor", function () {
  let contract;
  let owner;

  beforeEach(async function () {
    [owner] = await ethers.getSigners();
  const Lockb0xAnchor = await ethers.getContractFactory("Lockb0x_Anchor_Eth");
  contract = await Lockb0xAnchor.deploy();
    await contract.deployed();
  });

  it("should anchor a hash and retrieve it", async function () {
    const hash = ethers.keccak256(ethers.toUtf8Bytes("test"));
    const metadata = "{\"name\":\"test\"}";
    await contract.anchor(hash, metadata);
    const anchor = await contract.getAnchor(hash);
    expect(anchor.hash).to.equal(hash);
    expect(anchor.metadata).to.equal(metadata);
    expect(anchor.submitter).to.equal(owner.address);
    expect(anchor.timestamp).to.be.gt(0);
  });

  it("should not allow duplicate anchors", async function () {
    const hash = ethers.keccak256(ethers.toUtf8Bytes("test"));
    await contract.anchor(hash, "meta");
    await expect(contract.anchor(hash, "meta2")).to.be.revertedWith("Already anchored");
  });

  it("should revert if anchor not found", async function () {
    const hash = ethers.keccak256(ethers.toUtf8Bytes("missing"));
    await expect(contract.getAnchor(hash)).to.be.revertedWith("Not anchored");
  });
});
