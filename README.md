# 📝 ConfluenceRAG 

[![Build](https://github.com/aherrick/confluencerag/actions/workflows/build.yml/badge.svg)](https://github.com/aherrick/confluencerag/actions/workflows/build.yml)

ConfluenceRAG is a retrieval-augmented generation (RAG) service for embedding and searching Confluence content. It uses Azure OpenAI embeddings with a pluggable **vector store backend**.

---

## 🚀 Features

- 🔑 **Azure OpenAI Embeddings** – Generate high-quality embeddings from Confluence pages.  
  - Only the following embedding models are supported:  
    - `text-embedding-3-small`  
    - `text-embedding-3-large`  

- 📦 **Vector Store Backends** – automatic selection in this order:  
  1. **Azure AI Search** – if the `AzureAISearch` section is configured  
  2. **SQLite (on-disk)** – if `SqliteVectorDBPath` is set  
  3. **In-Memory** – fallback if nothing else is configured  

- 🧩 **Semantic Chunking** – Splits large documents into embedding-friendly chunks.  
- 🔍 **Vector Search API** – Query embeddings for semantic similarity.  
- 💬 **Chat Completion Response** – After retrieving the most relevant documents, the service uses an Azure OpenAI **chat deployment** to generate a concise, natural answer grounded in the results.  

---

## ⚙️ Configuration

Settings come from user secrets.

### Example

```json
{
  "IndexName": "confluence-pages",
  "ConfluenceOrg": "my-org",

  "AzureOpenAI": {
    "Endpoint": "https://xxx.openai.azure.com/",
    "EmbeddingDeployment": "text-embedding-3-small",
    "ChatDeployment": "gpt-4o-mini",
    "ApiKey": "your-key"
  },

  "AzureAISearch": {
    "Endpoint": "https://xxx.search.windows.net/",
    "ApiKey": "your-key"
  },

  "SqliteVectorDBPath": "vectorstore.db"
}
