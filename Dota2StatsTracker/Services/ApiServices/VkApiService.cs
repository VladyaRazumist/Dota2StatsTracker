using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2StatsTracker.Config;
using Microsoft.Extensions.Options;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace Dota2StatsTracker.Services.ApiServices
{
    public class VkApiService
    {
        private const string GroupName = "freishmejt";

        private readonly VkApi _vkApiClient;

        public VkApiService(IOptions<Settings> settings)
        {
            _vkApiClient = new VkApi();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _vkApiClient.Authorize(new ApiAuthParams
            {
                AccessToken = settings.Value.VkApiToken,
            });
        }

        public async Task<Post> GetRandomPost()
        {
            var groupResponse = (await _vkApiClient.Groups.SearchAsync(new GroupsSearchParams { Query = GroupName, Count = 1 })).First();
            var wallResponse = await _vkApiClient.Wall.GetAsync(new WallGetParams { OwnerId = -groupResponse.Id, Count = 1 });

            var postsCount = wallResponse.TotalCount;

            var rnd = new Random();
            var rndNumber = rnd.Next(1, (int)postsCount - 1);

            var randomPost = (await _vkApiClient.Wall.GetAsync(new WallGetParams { OwnerId = -groupResponse.Id, Count = 1, Offset = (ulong)rndNumber, Extended = true })).WallPosts[0];

            while (randomPost.Attachment == null || randomPost.Attachment.Type.Name != "Photo")
            {
                randomPost = (await _vkApiClient.Wall.GetAsync(new WallGetParams { OwnerId = -groupResponse.Id, Count = 1, Offset = (ulong)rndNumber, Extended = true })).WallPosts[0];
            }

            return new Post
            {
                Description = randomPost.Text,
                LikesCount = randomPost.Likes.Count,
                CommentsCount = randomPost.Comments.Count,
                AttachmentUrl = (randomPost.Attachment.Instance as Photo).Sizes.OrderByDescending(s => s.Height).First().Url.ToString(),
            };
        }

        public class Post
        {
            public string Description { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public string AttachmentUrl { get; set; }
        }
    }
}
