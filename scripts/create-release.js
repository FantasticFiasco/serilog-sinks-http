const fs = require("fs").promises;
const path = require("path");

const ARTIFACTS_PATH = "./artifacts";

const createRelease = async ({github, context}) => {
  const owner = context.repo.owner;
  const repo = context.repo.repo;
  const tag_name = context.ref.replace(/refs\/tags\//, '');

  const res = github.rest.repos.createRelease({
    owner,
    repo,
    tag_name,
    name: "Release " + tag_name,
    body: "TODO",
    draft: true,
  });

  const artifacts = await fs.readdir(ARTIFACTS_PATH);

  for (let artifact of artifacts) {
    const artifactPath = path.join(ARTIFACTS_PATH, artifact);
    const data = await fs.readFile(artifactPath);

    await github.rest.repos.uploadReleaseAsset({
      owner,
      repo,
      release_id: res.data.id,
      name: artifact,
      data
    });
  }
}

module.exports = createRelease;
